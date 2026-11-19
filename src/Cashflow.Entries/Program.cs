using Cashflow.Entries;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var module = new EntriesModule();
module.ConfigureServices(builder.Services, builder.Configuration);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

module.MapEndpoints(app);

app.Run();