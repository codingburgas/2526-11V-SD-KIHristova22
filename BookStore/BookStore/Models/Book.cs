using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BookStore.Models;
public class Book : SoftDeletableEntity
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    [Required]
    [Range(typeof(decimal), "0", "1000")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}