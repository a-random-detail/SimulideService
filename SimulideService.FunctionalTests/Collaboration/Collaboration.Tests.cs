using System.Net;
using Microsoft.AspNetCore.SignalR.Client;
using SimulideService.Domain;
using SimulideService.Domain.Contracts;
using SimulideService.Domain.Data;
using SimulideService.Response;

namespace SimulideService.FunctionalTests.Collaboration;

[TestFixture]
public class CollaborationTests
{
   private Domain.Data.Document _createdDocument;
   
   
   [SetUp]
   public async Task Setup()
   {
         var response = await Application.Host!.Scenario(_ =>
         {
              _.Post.Json(new PostDocumentRequest
              {
                Name = "Test Name",
                Content = "Test Content \n Here \n\t There"
              }).ToUrl("/documents");
              _.StatusCodeShouldBe(201); // Created
         });
         
         var responsePayload = response.ReadAsJson<ServiceResponse<Domain.Data.Document>>();
         Assert.That(responsePayload.Data, Is.InstanceOf<Domain.Data.Document>());
         _createdDocument = responsePayload.Data;
         
   }
   
   [Test]
   public async Task MultipleClients_CanJoinDocumentGroup_AndReceiveMessages()
   {
      var client1 = CreateHubConnection(); 
      var client2 = CreateHubConnection(); 

      List<Operation> client1Operations = [];
      List<Operation> client2Operations = [];
      
      List<CollabUserEvent> client1Events = [];
      List<CollabUserEvent> client2Events = [];
      
      client1.On<Operation>("ReceiveOperation", client1Operations.Add);
      client2.On<Operation>("ReceiveOperation", client2Operations.Add);
      
      client1.On<CollabUserEvent>("PartyChanged", client1Events.Add);
      client2.On<CollabUserEvent>("PartyChanged", client2Events.Add);

      await client1.StartAsync();
      await client2.StartAsync();
      
      var client1Connect = await client1.InvokeAsync<ServiceResponse<CollabSessionStatus>>(
         "JoinDocumentGroup", 
         _createdDocument.Id);
      
      Assert.That(client1Connect.IsSuccessful, Is.True);

      var client2Connect = await client2.InvokeAsync<ServiceResponse<CollabSessionStatus>>(
         "JoinDocumentGroup", 
         _createdDocument.Id);
      Assert.That(client2Connect.IsSuccessful, Is.True);

      await Task.Delay(500);
      
      Assert.That(client1Events.Count, Is.EqualTo(2));
      Assert.That(client2Events.Count, Is.EqualTo(1));
      
      Assert.That(client1Events.Find(x => x.ConnectionId == client1Connect.Data?.ConnectionId), Is.Not.Null);
      Assert.That(client1Events.Find(x => x.ConnectionId == client2Connect.Data?.ConnectionId), Is.Not.Null);

      Assert.That(client2Events.Find(x => x.ConnectionId == client2Connect.Data?.ConnectionId), Is.Not.Null);

      var operation1 = new ApplyOperationPayload 
      {
         DocumentId = _createdDocument.Id,
         Type = OperationType.Insert,
         Position = 0,
         Content = "Hello, World!",
         Version = _createdDocument.Version + 1,
         Length = 13
      };

      var expected = Operation.FromRequest(operation1);

      await client1.InvokeAsync("ApplyOperation", operation1);

      await Task.Delay(500);
      
      Assert.That(client1Operations.Count, Is.EqualTo(1));
      Assert.That(client1Operations[0].Position, Is.EqualTo(expected.Position));
      Assert.That(client1Operations[0].Content, Is.EqualTo(expected.Content));
      Assert.That(client1Operations[0].Version, Is.EqualTo(expected.Version));

      Assert.That(client2Operations.Count, Is.EqualTo(1));
      Assert.That(client2Operations[0].Position, Is.EqualTo(expected.Position));
      Assert.That(client2Operations[0].Content, Is.EqualTo(expected.Content));
      Assert.That(client2Operations[0].Version, Is.EqualTo(expected.Version));

      await client1.StopAsync();
      await client2.StopAsync();
   }

   [Test]
   public async Task ApplyDocument_UpdatesDocumentContentAndVersion() 
   {
      var client1 = CreateHubConnection(); 

      List<Operation> client1Operations = [];
      
      List<CollabUserEvent> client1Events = [];
      
      client1.On<Operation>("ReceiveOperation", client1Operations.Add);
      
      client1.On<CollabUserEvent>("PartyChanged", client1Events.Add);

      await client1.StartAsync();
      
      var client1Connect = await client1.InvokeAsync<ServiceResponse<CollabSessionStatus>>(
         "JoinDocumentGroup", _createdDocument.Id);
      
      Assert.That(client1Connect.IsSuccessful, Is.True);
      
      await Task.Delay(500);
      
      Assert.That(client1Events.Count, Is.EqualTo(1));
      Assert.That(client1Events.Find(x => x.ConnectionId == client1Connect.Data?.ConnectionId), Is.Not.Null);


      var operation1 = new ApplyOperationPayload 
      {
         DocumentId = _createdDocument.Id,
         Type = OperationType.Insert,
         Position = 0,
         Content = "Hello, World!",
         Version = 2,
         Length = 13
      };

      var expected = Operation.FromRequest(operation1);

      await client1.InvokeAsync("ApplyOperation", operation1);

      await Task.Delay(500);
      
      Assert.That(client1Operations.Count, Is.EqualTo(1));
      Assert.That(client1Operations[0].Position, Is.EqualTo(expected.Position));
      Assert.That(client1Operations[0].Content, Is.EqualTo(expected.Content));
      Assert.That(client1Operations[0].Version, Is.EqualTo(expected.Version));
      
      await client1.StopAsync();

      var response = await Application.Host!.Scenario(void (_) =>
      {
         _.Get.Url($"/documents/{_createdDocument.Id}");
         _.StatusCodeShouldBe(200); // Create
      });

      var responsePayload = response.ReadAsJson<ServiceResponse<Domain.Data.Document>>();
      Assert.That(responsePayload.Data, Is.InstanceOf<Domain.Data.Document>());
      var document = responsePayload.Data;
      Assert.That(document.Id, Is.EqualTo(_createdDocument.Id));
      Assert.That(document.Content, Is.EqualTo($"{expected.Content}{_createdDocument.Content}"));
      Assert.That(document.Version, Is.EqualTo(_createdDocument.Version + 1));
      Assert.That(document.CreatedAt, Is.EqualTo(_createdDocument.CreatedAt));
      Assert.That(document.UpdatedAt, Is.GreaterThan(_createdDocument.UpdatedAt));
      Assert.That(document.UpdatedAt, Is.GreaterThan(expected.CreatedAt));
   }

   [Test] 
   public async Task JoiningNonExistentDocument_ReturnsNotFound()
   {
      var client = CreateHubConnection(); 
      var invalidDocumentId = Guid.NewGuid().ToString();

      await client.StartAsync();

      var response = await client.InvokeAsync<ServiceResponse<CollabSessionStatus>>("JoinDocumentGroup", invalidDocumentId);
      
      Assert.That(response.IsSuccessful, Is.False);
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
      Assert.That(response.Data, Is.Null);
      Assert.That(response.Errors, Is.Not.Null);
      Assert.That(response.Errors.First().Message, Is.EqualTo($"Document with id {invalidDocumentId} not found."));;

      await client.StopAsync();
   }
   
   [Test] 
   public async Task ApplyOperation_ReturnsBadRequest_WithWrongVersionNumber()
   {
      var client = CreateHubConnection(); 

      await client.StartAsync();

      var clientConnect = await client.InvokeAsync<ServiceResponse<CollabSessionStatus>>("JoinDocumentGroup", _createdDocument.Id);
      
      List<Operation> clientMessages = [];
      
      client.On<Operation>("ReceiveOperation", operation => clientMessages.Add(operation));
      Assert.That(clientConnect.IsSuccessful, Is.True);

      var operation1 = new ApplyOperationPayload 
      {
         DocumentId = _createdDocument.Id,
         Type = OperationType.Insert,
         Position = 0,
         Content = "Hello, World!",
         Version = 16,
         Length = 13
      };

      var response = await client.InvokeAsync<ServiceResponse<CollabSessionStatus>>("ApplyOperation", operation1);

      await Task.Delay(500);
      
      Assert.That(response.IsSuccessful, Is.False);
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      Assert.That(response.Data, Is.Null);
      Assert.That(response.Errors, Is.Not.Null);
      Assert.That(response.Errors.First().Message, Is.EqualTo("Version is invalid. Expected version 1, got 16."));;

      Assert.That(clientMessages, Is.Empty);
      
      await client.StopAsync();
   }

   private HubConnection CreateHubConnection()
   {
      return new HubConnectionBuilder()
         .WithUrl(Application.CollaborationHubUrl,
         o => o.HttpMessageHandlerFactory = _ => Application.Host!.Server.CreateHandler())
         .Build();
   }
   
   
}