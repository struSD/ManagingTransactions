using ManagingTransaction.Api.Cofiguration;
using MediatR;
using ManagingTransactions.Domain.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ManagingTransaction.Domain.Commands;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using System.Reflection;
using System.IO;
using ManagingTransactions.Domain.Queries;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();



builder.Services.AddMediatR(typeof(ExportTransactionsQuery));
builder.Services.AddMediatR(typeof(GetTransactionsQuery));
builder.Services.AddMediatR(typeof(TransactionImpotUpdateCommand));
builder.Services.AddMediatR(typeof(UpdateTransactionStatusCommand));

builder.Services.Configure<AppConfiguration>(builder.Configuration);
builder.Services.AddDbContext<ManagingTransactionsDbContext>((sp, options) =>
{
    var configuration = sp.GetRequiredService<IOptionsMonitor<AppConfiguration>>();
    options.UseNpgsql(configuration.CurrentValue.ConnectionString);
});

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = "struSD",
            ValidAudience = "https://localhost:5014",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("tnNgZGbpcq7PDEJ3RGHXw6WdDbs28mM3"))
        };
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Managing Transactions API",
        Description = "Web API for managing transactions has been created by struSD",
        Contact = new OpenApiContact
        {
            Name = "github",
            Url = new Uri("https://github.com/struSD")
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});
}
app.UseAuthorization();
app.MapControllers();
app.Run();
