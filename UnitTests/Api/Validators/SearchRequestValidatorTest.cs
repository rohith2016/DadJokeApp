using Api.Validations;
using Application.DTOs.Search;
using FluentValidation.TestHelper;

namespace UnitTests.Api.Validators
{
    public class SearchRequestValidatorTests
    {
        private readonly SearchRequestValidator _validator;

        public SearchRequestValidatorTests()
        {
            _validator = new SearchRequestValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Term_Is_Empty()
        {
            var model = new SearchRequestDTO { Term = "", Limit = 10 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Term)
                  .WithErrorMessage("Search term is required");
        }

        [Fact]
        public void Should_Have_Error_When_Term_Too_Long()
        {
            var model = new SearchRequestDTO
            {
                Term = new string('a', 101),
                Limit = 10
            };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Term)
                  .WithErrorMessage("Search term too long");
        }

        [Fact]
        public void Should_Have_Error_When_Limit_Is_Too_Low()
        {
            var model = new SearchRequestDTO { Term = "funny", Limit = 0 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Limit)
                  .WithErrorMessage("Limit must be between 1 and 50");
        }

        [Fact]
        public void Should_Have_Error_When_Limit_Is_Too_High()
        {
            var model = new SearchRequestDTO { Term = "funny", Limit = 100 };
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Limit)
                  .WithErrorMessage("Limit must be between 1 and 50");
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Input()
        {
            var model = new SearchRequestDTO { Term = "dad", Limit = 10 };
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
