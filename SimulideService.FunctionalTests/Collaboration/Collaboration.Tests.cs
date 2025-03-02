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
   private string _hubUrl; 
   private Guid _documentId;
   
   
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
         _documentId = responsePayload.Data.Id;
         
         _hubUrl = $"{Application.BaseUrl}/ws/collaboration";
   }
   
   [Test]
   public async Task MultipleClients_CanJoinDocumentGroup_AndReceiveMessages()
   {
      var client1 = new HubConnectionBuilder().WithUrl(_hubUrl).Build();
      var client2 = new HubConnectionBuilder().WithUrl(_hubUrl).Build();

      List<Operation> client1Operations = [];
      List<Operation> client2Operations = [];
      
      List<CollabUserEvent> client1Events = [];
      List<CollabUserEvent> client2Events = [];
      
      client1.On<Operation>("ReceiveOperation", operation => client1Operations.Add(operation));
      client2.On<Operation>("ReceiveOperation", operation => client2Operations.Add(operation));
      
      client1.On<CollabUserEvent>("PartyChanged", userEvent => client1Events.Add(userEvent));
      client1.On<CollabUserEvent>("PartyChanged", userEvent => client2Events.Add(userEvent));

      await client1.StartAsync();
      await client2.StartAsync();
      
      var client1Connect = await client1.InvokeAsync<ServiceResponse<CollabSessionStatus>>(
         "JoinDocumentGroup", _documentId);
      var client2Connect = await client2.InvokeAsync<ServiceResponse<CollabSessionStatus>>(
         "JoinDocumentGroup", _documentId);
      
      Assert.That(client1Connect.IsSuccessful, Is.True);
      Assert.That(client2Connect.IsSuccessful, Is.True);
      
      await Task.Delay(500);
      
      Assert.That(client1Events.Count, Is.EqualTo(2));
      Assert.That(client2Events.Count, Is.EqualTo(2));
      
      Assert.That(client1Events.Find(x => x.ConnectionId == client1Connect.Data?.ConnectionId), Is.Not.Null);
      Assert.That(client1Events.Find(x => x.ConnectionId == client2Connect.Data?.ConnectionId), Is.Not.Null);

      Assert.That(client2Events.Find(x => x.ConnectionId == client1Connect.Data?.ConnectionId), Is.Not.Null);
      Assert.That(client2Events.Find(x => x.ConnectionId == client2Connect.Data?.ConnectionId), Is.Not.Null);

      var operation1 = new ApplyOperationPayload 
      {
         DocumentId = _documentId,
         Type = OperationType.Insert,
         Position = 0,
         Content = "Hello, World!",
         Version = 1,
         Length = 13
      };

      await client1.InvokeAsync("ApplyOperation", operation1);

      await Task.Delay(500);
      
      Assert.Contains(operation1, client2Operations);
      Assert.Contains(operation1, client1Operations);

      await client1.StopAsync();
      await client2.StopAsync();
   }
   
   [Test] 
   public async Task JoiningNonExistentDocument_ReturnsNotFound()
   {
      var client = new HubConnectionBuilder().WithUrl(_hubUrl).Build();
      var invalidDocumentId = Guid.NewGuid().ToString();

      await client.StartAsync();

      var response = await client.InvokeAsync<ServiceResponse<CollabSessionStatus>>("JoinDocumentGroup", invalidDocumentId);
      
      Assert.That(response.IsSuccessful, Is.False);
      Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
      Assert.That(response.Data, Is.Null);
      Assert.That(response.Errors, Is.Not.Null);
      Assert.That(response.Errors.First().Message, Is.EqualTo($"Document with id {invalidDocumentId} not found"));;

      await client.StopAsync();
   }
   
   [Test] 
   public async Task ApplyOperation_ReturnsBadRequest_WithWrongVersionNumber()
   {
      var client = new HubConnectionBuilder().WithUrl(_hubUrl).Build();

      await client.StartAsync();

      var clientConnect = await client.InvokeAsync<ServiceResponse<CollabSessionStatus>>("JoinDocumentGroup", _documentId);
      
      List<Operation> clientMessages = [];
      
      client.On<Operation>("ReceiveOperation", operation => clientMessages.Add(operation));
      Assert.That(clientConnect.IsSuccessful, Is.True);

      var operation1 = new ApplyOperationPayload 
      {
         DocumentId = _documentId,
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
      Assert.That(response.Errors.First().Message, Is.EqualTo($"Operation out of sync with document {_documentId}."));;

      Assert.That(clientMessages, Is.Empty);
      
      await client.StopAsync();
   }
   
   
}