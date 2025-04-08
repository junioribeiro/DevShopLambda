using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Upload.S3.API.Application.Domain;
using Upload.S3.API.Application.Infrastructure;
using Upload.S3.API.Application.Infrastructure.Repositories;
using Upload.S3.API.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<AWSCredentialSetup>(builder.Configuration.GetSection("AWSCredentialSetup"));

builder.Services.AddScoped<DbSession>();
builder.Services.AddScoped<IArchiveRepository, ArchiveRepository>();
builder.Services.AddScoped<IAmazonS3Service, AmazonS3Service>();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
