using FluentValidation;
using AU.Domain;

namespace AU.Application.Organizations.Commands.CreateOrganization;

public class CreateOrganizationValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(100);

        RuleFor(x => x.Slug!)
            .Length(OrganizationSlug.MinLength, OrganizationSlug.MaxLength)
            .Matches(OrganizationSlug.Pattern)
            .WithMessage("The slug can only contain lowercase letters, digits and single hyphens.")
            .When(x => x.Slug is not null);
    }
}
