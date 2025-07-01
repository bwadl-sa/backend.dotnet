using Bwadl.Application.Features.Users.Commands.UpdateUser;
using FluentValidation;

namespace Bwadl.Application.Validators;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid user type");
    }
}
