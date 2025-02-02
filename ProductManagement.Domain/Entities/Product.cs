using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Domain.Entities;

public class Product
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    
    public string Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public int UserId { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public bool IsDeleted { get; set; }
}