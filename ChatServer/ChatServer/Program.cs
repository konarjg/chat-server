

using ChatServer;
using Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddDomain();
builder.Services.AddRpcs(builder.Configuration);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.MapRpcs();
app.MapGet("/",() => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
