using FluentValidation;

namespace TaskHub.Api.Models.Dtos.Boards;

public class CreateBoardDtoValidator : AbstractValidator<CreateBoardDto>
{
    public CreateBoardDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Board title is required")
            .Length(1, 100).WithMessage("Board title must be between 1 and 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Board description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
} 