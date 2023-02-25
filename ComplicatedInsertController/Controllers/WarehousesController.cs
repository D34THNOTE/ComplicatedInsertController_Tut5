using System.Data.SqlClient;
using ComplicatedInsertController.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ComplicatedInsertController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehousesController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post([FromBody] IncomingData incomingData)
        {

            using (var connection = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;"))
            {
                try
                {
                    connection.Open();
                    
                    // Checking if the product Id exists
                    using (var command = new SqlCommand("SELECT COUNT(*) FROM Product WHERE IdProduct = @id", connection))
                    {
                        command.Parameters.AddWithValue("@id", incomingData.IdProduct);
                        int productCheck;
                        try
                        {
                            productCheck = (int)command.ExecuteScalar();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            return StatusCode(500, "There was an error verifying the provided product ID");
                        }

                        if (productCheck == 0)
                        {
                            return StatusCode(404,$"Product with ID {incomingData.IdProduct} not found");
                        }
                    }
                    
                    // Checking if the WarehouseId exists
                    using (var command = new SqlCommand("SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @id", connection))
                    {
                        command.Parameters.AddWithValue("@id", incomingData.IdWarehouse);
                        int warehouseCheck;
                        try
                        {
                            warehouseCheck = (int)command.ExecuteScalar();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            return StatusCode(500, "There was an error verifying the provided warehouse ID");
                        }

                        if (warehouseCheck == 0)
                        {
                            return StatusCode(404,$"Warehouse with ID {incomingData.IdWarehouse} not found");
                        }
                    }
                    
                    int? orderId; // this variable will be useful in the next SqlCommand so I'm saving it outside of the "using" block
                    
                    // Checking if the order is matching the record in Order table
                    using (var command = new SqlCommand("SELECT IdOrder FROM [Order] WHERE IdProduct = @idProduct AND Amount = @amount AND CreatedAt <= @createdAt", 
                               connection))
                    {
                        command.Parameters.AddWithValue("@idProduct", incomingData.IdProduct);
                        command.Parameters.AddWithValue("@amount", incomingData.Amount);
                        command.Parameters.AddWithValue("@createdAt", incomingData.CreatedAt);
                        
                        try
                        {
                            orderId = (int?)command.ExecuteScalar();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            return StatusCode(500, "There was an error verifying the provided order details");
                        }

                        if (orderId == null)
                        {
                            return StatusCode(404,$"No matching order found for product with ID {incomingData.IdProduct}, the amount {incomingData.Amount} and a correct creation date");
                        }

                        // check if the order has been already completed in Product_Warehouse
                        using (var checkCommand = new SqlCommand("SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @idOrder", connection))
                        {
                            checkCommand.Parameters.AddWithValue("@idOrder", orderId);
                            int productWarehouseCheck;
                            try
                            {
                                productWarehouseCheck = (int)checkCommand.ExecuteScalar();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                return StatusCode(500, "There was an error verifying whether the order has already been completed");
                            }

                            if (productWarehouseCheck > 0)
                            {
                                return StatusCode(404,$"Order with ID {orderId} has already been fulfilled");
                            }
                        }
                    }

                    // fetching the price of the Product
                    decimal productPrice;
                    using (var command = new SqlCommand("SELECT Price FROM Product WHERE IdProduct = @idProduct", connection))
                    {
                        command.Parameters.AddWithValue("@idProduct", incomingData.IdProduct);
                        try
                        {
                            productPrice = Convert.ToDecimal(command.ExecuteScalar());
                        } catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            return StatusCode(500, "There was an error when fetching the price of the product");
                        }
                        
                    }
                    
                    // using transaction to ensure data safety in case of an error
                    using (var transaction = connection.BeginTransaction())
                    {
                        int? newRecordId;
                        
                        try
                        {
                           using (var command = new SqlCommand("UPDATE [Order] SET FulfilledAt = @fulfilledAt WHERE IDorder = @idOrder", 
                                      connection, transaction))
                        {
                            command.Parameters.AddWithValue("@fulfilledAt", DateTime.Now);
                            command.Parameters.AddWithValue("@idOrder", orderId);
                            try
                            {
                                command.ExecuteNonQuery();
                            } catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                return StatusCode(500, "There was an error when updating the order, no rows were affected");
                            }
                        } 
                           
                        // OUTPUT will return the value of IdProductWarehouse
                        using (var command = new SqlCommand("INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) " +
                                                            "OUTPUT INSERTED.IdProductWarehouse VALUES (@idWarehouse, @idProduct, @idOrder, @amount, @price, @createdAt)", 
                                   connection, transaction))
                        {
                            command.Parameters.AddWithValue("@idWarehouse", incomingData.IdWarehouse);
                            command.Parameters.AddWithValue("@idProduct", incomingData.IdProduct);
                            command.Parameters.AddWithValue("@idOrder", orderId);
                            command.Parameters.AddWithValue("@amount", incomingData.Amount);
                            command.Parameters.AddWithValue("@price", incomingData.Amount * productPrice);
                            command.Parameters.AddWithValue("@createdAt", DateTime.Now);

                            
                            try
                            {
                                newRecordId = (int)command.ExecuteScalar();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                return StatusCode(500, "There was an error when inserting a record into the database, no rows were affected");
                            }
                            
                        }
                        
                        transaction.Commit(); 
                        return Ok(newRecordId);
                        
                        } catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            return StatusCode(500, "Internal error occured, no rows were affected");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return StatusCode(405, "Unexpected error");
        }
    }
}