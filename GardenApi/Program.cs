using Microsoft.EntityFrameworkCore;
using GardenBusiness;

//main CORS source: https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-10.0
//declaring name of policy for CORS (Cross Origin Resource Sharing) policy
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//defining CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("https://localhost:7267", "http://localhost:5005") //DO NOT use a trailing slash--4 sources stress
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

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

//enabling routing for the application (needed for CORS to work)
//app.UseRouting(); //.NET adds it automatically, adding it again causes issues

app.UseHttpsRedirection();

//enabling CORS with named policy
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();

//things to explore: is it more secure to use endpoint routing instead of a named policy?


