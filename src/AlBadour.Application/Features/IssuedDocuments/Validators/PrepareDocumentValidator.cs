using AlBadour.Application.Features.IssuedDocuments.Commands;
using FluentValidation;

namespace AlBadour.Application.Features.IssuedDocuments.Validators;

public class PrepareDocumentValidator : AbstractValidator<PrepareDocumentCommand>
{
    public PrepareDocumentValidator()
    {
        RuleFor(x => x.Dto.RequestId)
            .NotEmpty().WithMessage("Request ID is required.");
    }
}
