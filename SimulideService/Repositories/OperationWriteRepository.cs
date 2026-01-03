using System.Text;
using SimulideService.Domain.Data;

namespace SimulideService.Repositories;

public interface IOperationWriteRepository
{
    Task ApplyOperationToDocument(CollabContext dbContext, Operation appliedOperation, Document document);
    Task<Operation> CreateAsync(CollabContext dbContext, Operation operation); 
}

public class OperationWriteRepository : IOperationWriteRepository 
{
   public Task ApplyOperationToDocument(CollabContext dbContext, Operation appliedOperation, Document document)
   {
      appliedOperation.Version = document.Version+1;
      document.Content = ApplyOperationToDocumentContent(document.Content, appliedOperation); 
      
      document.Version = appliedOperation.Version;
      document.UpdatedAt = appliedOperation.CreatedAt;
      dbContext.Documents.Update(document);
      return Task.CompletedTask;
   }


    public async Task<Operation> CreateAsync(CollabContext dbContext, Operation operation)
   {
      dbContext.Operations.Add(operation);
      return operation;
   }

   private string ApplyOperationToDocumentContent(string? content, Operation appliedOperation)
   {
      var sb = new StringBuilder(content);
      if (appliedOperation.Type == OperationType.Delete)
      {
         sb.Remove(appliedOperation.Position, appliedOperation.Length);
      }
      else if (appliedOperation.Type == OperationType.Insert)
      {
         sb.Insert(appliedOperation.Position, appliedOperation.Content);
      }

      return sb.ToString();
   }
}
