using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimulideService.Domain.Contracts;
using SimulideService.Domain.Data;
using SimulideService.FunctionalTests.DB;
using SimulideService.Repositories;
using SimulideService.Response;

namespace SimulideService.FunctionalTests.Document;

[TestFixture]
public class PostDocument
{

    [Test]
    public async Task PostDocument_ReturnsOK_WithValidData()
    {
        var testStartTime = DateTime.UtcNow;
        
        var response = await Application.Host.Scenario(_ =>
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
        var document = responsePayload.Data;
        Assert.That(document.Name, Is.EqualTo("Test Name"));
        Assert.That(document.Content, Is.EqualTo("Test Content \n Here \n\t There"));
        Assert.That(document.Version, Is.EqualTo(1));
        Assert.That(document.CreatedAt, Is.GreaterThan(testStartTime));
        Assert.That(document.CreatedAt, Is.LessThan(DateTime.Now.ToUniversalTime()));
        Assert.That(document.CreatedAt, Is.EqualTo(document.UpdatedAt));
    }

    [Test]
    public async Task PostDocument_ReturnsBadRequest_WithNoName()
    {
        var response = await Application.Host.Scenario(_ =>
        {
            _.Post.Json(new PostDocumentRequest
            {
                Name = "",
                Content = "Test Content \n Here \n\t There"
            }).ToUrl("/documents");
            _.StatusCodeShouldBe(400); // BadRequest
        });
        
        var payload = response.ReadAsJson<ServiceResponse<Domain.Data.Document>>();
        
        Assert.That(payload.Success, Is.False);
        Assert.That(payload.Errors, Is.Not.Null);
        Assert.That(payload.Errors.Count, Is.EqualTo(1));
        Assert.That(payload.Errors[0].Message, Is.EqualTo("Name is required"));
    }
    
    [Test]
    public async Task PostDocument_ReturnsInternalServerError_WithDatabaseIssue()
    {
        var errorHost = await AlbaHost.For<Program>(x =>
        {
            x.UseEnvironment("Test");

            x.ConfigureServices((context, services) =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CollabContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddScoped<IDocumentWriteRepository, ErrorDocumentWriteRepository>();
                services.AddDbContext<CollabContext>(options =>
                    options.UseNpgsql(Application.PostgreSqlContainer!.GetConnectionString()));

                // Apply migrations
                using var scope = services.BuildServiceProvider().CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<CollabContext>();
                dbContext.Database.EnsureCreated();
            });
        }); 
        var response = await errorHost.Scenario(_ =>
        {
            _.Post.Json(new PostDocumentRequest
            {
                Name = "This is a valid name",
                Content = "Content \n also \n\t valid"
            }).ToUrl("/documents");
            _.StatusCodeShouldBe(500); // BadRequest
        });
        
        var payload = response.ReadAsJson<ServiceResponse<Domain.Data.Document>>();
        
        Assert.That(payload.Success, Is.False);
        Assert.That(payload.Errors, Is.Not.Null);
        Assert.That(payload.Errors.Count, Is.EqualTo(1));
        Assert.That(payload.Errors[0].Message, Is.EqualTo("An error occurred while processing the request."));
    }

}