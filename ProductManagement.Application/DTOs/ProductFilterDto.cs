namespace ProductManagement.Application.DTOs;

public record ProductFilterDto(string? Name, decimal? MinPrice, decimal? MaxPrice);