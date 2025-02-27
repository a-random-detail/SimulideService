using MediatR;
using SimulideService.Domain;
using SimulideService.Domain.Data;

namespace SimulideService.Repositories.Queries;

public class GetDocumentByIdQueryHandler(IDocumentReadRepository documentReadRepository) : IRequestHandler<GetDocumentByIdQuery, Either<Exception, Document>>
{
    public async Task<Either<Exception, Document>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
       return await documentReadRepository.GetDocumentByIdAsync(request.DocumentId); 
    }
}