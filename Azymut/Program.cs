using DataAccessLibrary.Models.Azymut;
using DataAccessLibrary.Data.Azymut;
using DataAccessLibrary.Databases;
using Azymut.Helpers;
using DataAccessLibrary.Data.Shoper;
using DataAccessLibrary.Data.Hangfire;
using RequestHelpersLibrary;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient to use the IHttpClientFactory and have a single instance of it
builder.Services.AddHttpClient();

// External libraries
builder.Services.AddSingleton<ISqlData, SqlData>();
builder.Services.AddSingleton<IHangfireDataService, HangfireDataService>();
builder.Services.AddSingleton<IAzymutDataService, AzymutDataService>();
builder.Services.AddSingleton<IAzymutService, AzymutService>();
builder.Services.AddSingleton<IShoperService, ShoperService>();
builder.Services.AddSingleton<IShoperDataService, ShoperDataService>();
builder.Services.AddSingleton<IRequestValidator, RequestValidator>();
builder.Services.AddTransient<IRequestAliveConnectionKeeper, RequestAliveConnectionKeeper>(); // Once more clients are on board, this might need to be changed to Singleton to save resources


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
