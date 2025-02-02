using FluentValidation;
using ProductManagement.Application.DTOs;

namespace ProductManagement.Application.Validators;

public class ProductFilterDtoValidator : AbstractValidator<ProductFilterDto>
{
    public ProductFilterDtoValidator()
    {
        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue).WithMessage("MinPrice must be greater than or equal to zero");

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue).WithMessage("MaxPrice must be greater than or equal to zero");

        RuleFor(x => x.MinPrice)
            .LessThanOrEqualTo(x => x.MaxPrice).When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
            .WithMessage("MinPrice cannot be greater than MaxPrice");
    }
}