using funzies.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();

// Register OpenAI service

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.UseRouting();
app.UseAuthorization();

app.MapGet(
        "/chat",
        async Task<IResult> (string message) =>
        {
            using var scope = app.Services.CreateAsyncScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var openAIService = scope.ServiceProvider.GetRequiredService<IOpenAIService>();
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return Results.BadRequest("Message is required");
                }

                var response = await openAIService.GenerateChatCompletionAsync(message);
                return Results.Ok(new { text = response });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing chat completion request");
                return Results.InternalServerError(
                    "An error occurred while processing your request"
                );
            }
        }
    )
    .WithName("GetChatCompletion")
    .Produces<string>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces<string>(StatusCodes.Status500InternalServerError)
    .WithOpenApi();

await app.RunAsync();
