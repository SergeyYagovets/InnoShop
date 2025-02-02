namespace ProductManagement.Application.DTOs;

public record ProductDto(
    int Id, string Name, string Description, decimal Price, bool IsAvailable, int UserId, DateTime CreatedDate);