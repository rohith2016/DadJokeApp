using Application.DTOs.Search;
using FluentValidation;

namespace Api.Validations
{
    public class SearchRequestValidator : AbstractValidator<SearchRequestDTO>
    {
        public SearchRequestValidator()
        {
            RuleFor(x => x.Term)
                .NotEmpty().WithMessage("Search term is required")
                .MaximumLength(100).WithMessage("Search term too long");

            RuleFor(x => x.Limit)
                .InclusiveBetween(1, 50).WithMessage("Limit must be between 1 and 50");
        }
    }
}
