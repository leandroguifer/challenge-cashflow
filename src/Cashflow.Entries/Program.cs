using Cashflow.Core.Documentation;
using Cashflow.Core.Security;
using Cashflow.Entries;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiDocumentation();

var module = new EntriesModule();
module.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapApiDocumentation();
}

app.UseApiSecurity();

module.MapEndpoints(app);

app.Run();
