using System.ComponentModel.DataAnnotations;
namespace BookStore.Models;
public class Category : SoftDeletableEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    public ICollection<Book> Books { get; set; } = new List<Book>();
}