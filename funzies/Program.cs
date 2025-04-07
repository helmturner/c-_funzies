using funzies.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<IOpenAIService, OpenAIService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable static files middleware
app.UseStaticFiles();

app.UseHttpsRedirection();
app.MapControllers();

app.UseRouting();
app.UseAuthorization();

// Serve the index.html as root
app.MapGet("/", () => Results.File("index.html", "text/html"));

app.MapPost(
        "/chat",
        async Task<IResult> (ChatRequest request) =>
        {
            using var scope = app.Services.CreateAsyncScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            var openAIService = scope.ServiceProvider.GetRequiredService<IOpenAIService>();
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return Results.BadRequest("Message is required");
                }

                var response = await openAIService.GenerateChatCompletionAsync(request.Message);
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

public partial class Program
{
    public Program() { }

    struct ChatRequest
    {
        public string Message { get; set; }
    }
}
