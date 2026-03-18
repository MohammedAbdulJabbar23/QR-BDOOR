using AlBadour.Application.Features.DocumentRequests.DTOs;
using FluentValidation;

namespace AlBadour.Application.Features.DocumentRequests.Validators;

public class CreateRequestValidator : AbstractValidator<Commands.CreateRequestCommand>
{
    public CreateRequestValidator()
    {
        RuleFor(x => x.Dto.PatientName)
            .MaximumLength(255).WithMessage("Patient name must not exceed 255 characters.");

        RuleFor(x => x.Dto.RecipientEntity)
            .NotEmpty().WithMessage("Recipient entity is required.")
            .MaximumLength(255).WithMessage("Recipient entity must not exceed 255 characters.");

        RuleFor(x => x.Dto.DocumentTypeId)
            .NotEmpty().WithMessage("Document type is required.");
    }
}
