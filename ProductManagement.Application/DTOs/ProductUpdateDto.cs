namespace ProductManagement.Application.DTOs;

public record ProductUpdateDto(string Name, string Description, decimal Price, bool IsAvailable);