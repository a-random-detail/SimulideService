using System.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SimulideService.Domain.Data;
using SimulideService.Repositories;
using SimulideService.Response;

namespace SimulideService.FunctionalTests.Document;

[TestFixture]
public class GetDocument
{

    [Test]
    public async Task GetDocument_ReturnsOK_WithValidData()
    {
        var expectedDocument = await PrepDocumentInDb();
        var response = await Application.Host!.Scenario(void (_) =>
        {
            _.Get.Url($"/documents/{expectedDocument.Id}");
            _.StatusCodeShouldBe(200); // Create
        });

        var responsePayload = response.ReadAsJson<ServiceResponse<Domain.Data.Document>>();
        Assert.That(responsePayload.Data, Is.InstanceOf<Domain.Data.Document>());
        var document = responsePayload.Data;
        Assert.That(document.Id, Is.EqualTo(expectedDocument.Id));
        Assert.That(document.Name, Is.EqualTo(expectedDocument.Name));
        Assert.That(document.Content, Is.EqualTo(expectedDocument.Content));
        Assert.That(document.Version, Is.EqualTo(expectedDocument.Version));
        Assert.That(document.CreatedAt, Is.EqualTo(expectedDocument.CreatedAt));
        Assert.That(document.UpdatedAt, Is.EqualTo(expectedDocument.UpdatedAt));
    }
    
    [Test]
    public async Task GetDocument_ReturnsNotFound_WithInvalidId()
    {
        await Application.Host!.Scenario(void (_) =>
        {
            _.Get.Url($"/documents/{Guid.NewGuid()}");
            _.StatusCodeShouldBe(404); // Create
        });
    }

    [Test]
    public async Task GetDocument_ReturnsInternalServerError_WhenDbError()
    {
        var expectedDocument = await PrepDocumentInDb();
        var errorHost = await AlbaHost.For<Program>(x =>
        {
            x.UseEnvironment("Test");

            x.ConfigureServices((context, services) =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDocumentReadRepository));
                if (descriptor != null) services.Remove(descriptor);

                var testConnectionString = Application.PostgreSqlContainer!.GetConnectionString();
                services.AddScoped<IDocumentReadRepository, ErrorDocumentReadRepository>();
                services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(testConnectionString));
            });
            
        }); 
        await errorHost.Scenario(void (_) =>
        {
            _.Get.Url($"/documents/{expectedDocument.Id}");
            _.StatusCodeShouldBe(500);
        });
    }

    private async Task<Domain.Data.Document> PrepDocumentInDb()
    {
        var testStartTime = DateTime.UtcNow;
        var expectedDocument = new Domain.Data.Document
        {
            Name = "Test Name",
            Content = "Test Content \n Here \n\t There",
            Version = 1,
            CreatedAt = testStartTime,
            UpdatedAt = testStartTime
        };

        using var scope = Application.Host!.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CollabContext>();

        await dbContext.Database.MigrateAsync();
        dbContext.Documents.Add(expectedDocument);
        await dbContext.SaveChangesAsync();
        return expectedDocument;
    }
}