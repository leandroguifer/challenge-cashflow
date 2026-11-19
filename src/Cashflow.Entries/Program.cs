using Cashflow.Entries;
using Cashflow.Core.Security;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var module = new EntriesModule();
module.ConfigureServices(builder.Services, builder.Configuration);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseWebCors();
app.UseAuthentication();
app.UseAuthorization();

module.MapEndpoints(app);

app.Run();