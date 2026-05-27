using FastEndpoints;
using FluentValidation;
using PropertyEval.Application.DTOs;

namespace PropertyEval.Web.Endpoints.Users;

public class LoginUserValidator : Validator<LoginUserRequest>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
