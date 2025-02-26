using System.Collections;
using SimulideService.Domain;
using SimulideService.Domain.Contracts;

namespace SimulideService.Validators;

public static class CreateDocumentRequestValidator
{
    public static Either<List<Exception>, PostDocumentRequest> FieldsAreValid(PostDocumentRequest documentRequest)
    {
        List<Exception> errors = [];

        if (string.IsNullOrWhiteSpace(documentRequest.Name))
            errors.Add(new DocumentValidationException("Name is required"));

        return errors.Any()
            ? Either<List<Exception>, PostDocumentRequest>.Left(errors)
            : Either<List<Exception>, PostDocumentRequest>.Right(documentRequest);
    }
    
}