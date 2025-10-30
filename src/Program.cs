using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
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
        "[DEMO][USER] Received user registration with Name: {Name} and Email: {Email}",
        request.Name,
        request.Email);

    var validationRequest = new ValidationRequest(
        Guid.NewGuid(),
        request.Name,
        request.Email);

    var client = factory.CreateClient("ValidationApi");
    var response = await client.PostAsJsonAsync(
        "api/validate",
        validationRequest,
        cancellationToken);

    var content = await response.Content.ReadAsStringAsync(cancellationToken);
    var responseData = JsonSerializer.Deserialize<ValidationResponse>(content, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    return Results.Ok(new
    {
        responseData?.Id,
        responseData?.Name,
        responseData?.Email,
        responseData?.Status
    });
});

app.MapPost("/webhook", async (ILogger<Program> logger, WebhookRequest request) =>
{
    logger.LogInformation(
        "[DEMO][WEBHOOK] Received webhook with: {Id}, {Name}, {Email}, {Status}, {Message}",
        request?.Id,
        request?.Name,
        request?.Email,
        request?.Status,
        request?.Message);

    return Results.NoContent();
});



await app.RunAsync();


public record RegisterRequest(
    string Name,
    string Email);

public record ValidationRequest(
    Guid Id,
    string Name,
    string Email);

public record ValidationResponse(
    Guid Id,
    string Name,
    string Email,
    string Status);

public record WebhookRequest(
    Guid Id,
    string Name,
    string Email,
    string Status,
    string Message);
