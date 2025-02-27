using MediatR;
using SimulideService.Domain;
using SimulideService.Domain.Data;

namespace SimulideService.Repositories.Queries;

public record GetDocumentByIdQuery(Guid DocumentId) : IRequest<Either<Exception, Document>>;