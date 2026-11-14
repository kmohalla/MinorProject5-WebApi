using Microsoft.EntityFrameworkCore;
using GardenBusiness;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//register connection string and context similar to Razor pages
var connectionString =
    builder.Configuration.GetConnectionString("gardening")
        ?? throw new InvalidOperationException("Connection string"
        + "'gardening' not found.");

builder.Services.AddDbContext<GardenData.GardenContext>(options =>
    options.UseSqlServer(connectionString));

//business managers
builder.Services.AddScoped<GardenBusiness.GardenBedManager>();
builder.Services.AddScoped<GardenBusiness.GuideManager>();
builder.Services.AddScoped<GardenBusiness.PlantManager>();
builder.Services.AddScoped<GardenBusiness.SupplierManager>();
builder.Services.AddScoped<GardenBusiness.ToolManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
