using Newtonsoft.Json;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://127.0.0.1:5500")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API 1");
    });
}


app.UseCors(MyAllowSpecificOrigins);


app.UseHttpsRedirection();


app.MapPost("/middleman", async (HttpContext context) =>
{
    var apiUrl = "https://localhost:7201/api/edamam-nutrition";
    var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();



    using var client = new HttpClient();
    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

    var response = await client.PostAsync(apiUrl, content);

    if (response.IsSuccessStatusCode)
    {
       
        var secondApiResponseContent = await response.Content.ReadAsStringAsync();

        // Renvoyez les données désérialisées dans la réponse HTTP
        context.Response.StatusCode = 200; // Statut OK
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(secondApiResponseContent);
    }
    else
    {
        // La deuxième API a renvoyé une erreur, renvoyez le code d'erreur approprié
        context.Response.StatusCode = (int)response.StatusCode;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync("Error from Second API: " + response.ReasonPhrase);
    }


});
app.Run();

