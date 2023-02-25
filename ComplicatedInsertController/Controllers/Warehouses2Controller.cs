using System.Data;
using System.Data.SqlClient;
using ComplicatedInsertController.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ComplicatedInsertController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Warehouses2Controller : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] IncomingData incomingData)
        {
            using (var connection = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;"))
            {
                try
                {
                    using (SqlCommand command = new SqlCommand("AddProductToWarehouse", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@IdProduct", SqlDbType.Int).Value = incomingData.IdProduct;
                        command.Parameters.Add("@IdWarehouse", SqlDbType.Int).Value = incomingData.IdWarehouse;
                        command.Parameters.Add("@Amount", SqlDbType.Int).Value = incomingData.Amount;
                        command.Parameters.Add("@CreatedAt", SqlDbType.DateTime).Value = incomingData.CreatedAt;

                        await connection.OpenAsync();
                        try
                        {
                            await command.ExecuteNonQueryAsync() ;
                            return Ok("Order was correctly added to the database");
                        }
                        catch (SqlException ex)
                        {
                            // if the SQL exception number is 50000 then it means that the stored procedure raised an error with a custom message 
                            if (ex.Number == 50000)
                            {
                                return StatusCode(404, ex.Message);
                            } 
                            Console.WriteLine(ex);
                            return StatusCode(500, "There was an error when processing your request, no rows were affected");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return StatusCode(500, "There was an internal error when connecting to the database");
                }
            }
        }
    }
}

