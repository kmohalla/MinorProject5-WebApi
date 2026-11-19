var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();


//app.MapGet("/", () => "Hello World!");

//enabling wwwroot folder to serve static files like index.html, css, js, images etc.
app.UseDefaultFiles();
app.UseStaticFiles(); //if static files are retrieved with javascript this goes after UseCors

app.Run();
