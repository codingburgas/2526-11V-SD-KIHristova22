using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace BookStore.Models;
public class OrderItem
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    [Range(1, 9999)]
    public int Quantity { get; set; }
    [Range(typeof(decimal), "0", "1000")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
}