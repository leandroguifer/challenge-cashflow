using Cashflow.Balance;
using Cashflow.Core.Documentation;
using Cashflow.Core.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiDocumentation();

var module = new BalanceModule();
module.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapApiDocumentation();
}

app.UseApiSecurity();

module.MapEndpoints(app);

app.Run();
