using System.Collections;
using SimulideService.Domain;
using SimulideService.Domain.Contracts;
using SimulideService.Domain.Data;

namespace SimulideService.Validators;

public static class ApplyOperationPayloadValidator
{
    public static Either<List<Exception>, ApplyOperationPayload> FieldsAreValid(ApplyOperationPayload request, Document? document)
    {
        List<Exception> errors = [];

        if (request.DocumentId == Guid.Empty || document is null || request.DocumentId != document.Id)
        {
            errors.Add(new OperationValidationException("DocumentId is missing or invalid."));
        }

        if (document != null && request.Version != document.Version)
        {
            errors.Add(new OperationValidationException("Version is invalid, please try again."));
        }

        if (request.Type == OperationType.None)
        {
            errors.Add(new OperationValidationException("Operation type is required."));
        }
        if (request.Length == 0)
        {
            errors.Add(new OperationValidationException("Length is required."));
        }

        return errors.Any()
            ? Either<List<Exception>, ApplyOperationPayload>.Left(errors)
            : Either<List<Exception>, ApplyOperationPayload>.Right(request);
    }
    
}