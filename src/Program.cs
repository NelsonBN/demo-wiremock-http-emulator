using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateSlimBuilder(args);


builder.Services.AddHttpClient("ValidationApi", (serviceProvider, client) =>
{
    var endpoint = serviceProvider.GetRequiredService<IConfiguration>()["ValidationEndpoint"] ?? throw new AggregateException();
    client.BaseAddress = new Uri(endpoint);
});


var app = builder.Build();

app.MapPost("/user",  async (IConfiguration configuration, ILogger<Program> logger, IHttpClientFactory factory, RegisterRequest request, CancellationToken cancellationToken) =>
{
    var endpoint = configuration["ValidationEndpoint"] ?? throw new AggregateException();

    logger.LogInformation(
        "Received user registration with Name: {Name} and Email: {Email}",
        request.Name,
        request.Email);

    var id = Guid.NewGuid();

    var client = factory.CreateClient("ValidationApi");
    var response = await client.GetFromJsonAsync<ValidationRequest>("api/validate", cancellationToken);

    return Results.Ok(new
    {
        Id = id,
        request.Name,
        request.Email,
        ValidationStatus = response?.Status
    });
});

app.MapPost("/webhook", (ILogger<Program> logger, WebhookRequest request) => {
    logger.LogInformation(
        "Received webhook with Message: {Message}",
        request.Message);
    var id = Guid.NewGuid();

    return Results.NoContent();
});


await app.RunAsync();


public record RegisterRequest(
    string Name,
    string Email);

public record ValidationRequest(string Status);

public record WebhookRequest(string Message);
