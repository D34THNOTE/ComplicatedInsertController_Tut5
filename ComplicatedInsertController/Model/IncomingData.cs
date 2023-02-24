using System.ComponentModel.DataAnnotations;

namespace ComplicatedInsertController.Model
{
    public class IncomingData
    {
        [Required(ErrorMessage = "ID of the product required")]
        public int IdProduct { get; set; }
        
        [Required(ErrorMessage = "ID of the warehouse required")]
        public int IdWarehouse { get; set; }
        
        [Required(ErrorMessage = "The amount of products is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be a positive value")]
        public int Amount { get; set; }
        
        [Required(ErrorMessage = "The amount of products is required")]
        public DateTime CreatedAt { get; set; }
    }
}