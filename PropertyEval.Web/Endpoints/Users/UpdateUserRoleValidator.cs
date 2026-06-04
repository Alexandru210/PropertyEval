using FastEndpoints;
using FluentValidation;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Constants;

namespace PropertyEval.Web.Endpoints.Users;

public class UpdateUserRoleValidator : Validator<UpdateUserRoleRequest>
{
    public UpdateUserRoleValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("User id must be valid.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => SystemRoles.AssignableRoles.Contains(role))
            .WithMessage("Role must be Client, Evaluator, or Admin.");
    }
}
