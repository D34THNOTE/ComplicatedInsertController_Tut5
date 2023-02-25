using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Setting a port just so I know where it is running
// Thic can be uncommented to automatically run Swagger UI which can be used for sending test requests, but I'm using Postman on port 6000 instead

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Loopback, 6000);
});

            

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// This made it so that neither http nor https responses were working through Postman so I commented it out
//app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();