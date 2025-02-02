namespace ProductManagement.Application.DTOs;

public record ProductCreateDto(string Name, string Description, decimal Price, bool IsAvailable);