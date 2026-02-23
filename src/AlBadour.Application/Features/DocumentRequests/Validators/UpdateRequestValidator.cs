using FluentValidation;

namespace AlBadour.Application.Features.DocumentRequests.Validators;

public class UpdateRequestValidator : AbstractValidator<Commands.UpdateRequestCommand>
{
    public UpdateRequestValidator()
    {
        RuleFor(x => x.Dto.PatientName)
            .NotEmpty().WithMessage("Patient name is required.")
            .MaximumLength(255).WithMessage("Patient name must not exceed 255 characters.");

        RuleFor(x => x.Dto.RecipientEntity)
            .NotEmpty().WithMessage("Recipient entity is required.")
            .MaximumLength(255).WithMessage("Recipient entity must not exceed 255 characters.");

        RuleFor(x => x.Dto.DocumentTypeId)
            .NotEmpty().WithMessage("Document type is required.");
    }
}
