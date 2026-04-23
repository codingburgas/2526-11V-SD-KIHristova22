using System.ComponentModel.DataAnnotations;

namespace BookStore.ViewModels;

public class CreateOrderViewModel
{
    [Required]
    public int BookId { get; set; }

    // Display-only fields for the Create form.
    public string BookTitle { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }

    [Range(1, 9999)]
    public int Quantity { get; set; } = 1;
}