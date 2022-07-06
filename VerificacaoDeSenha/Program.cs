global using Microsoft.EntityFrameworkCore;
global using VerificacaoDeSenha.Data;
global using VerificacaoDeSenha.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>
    (options => options.UseMySql(
        "server=localhost;initial catalog=UserLogin;uid=root;pwd=1234",
        Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.28-mysql")));


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
