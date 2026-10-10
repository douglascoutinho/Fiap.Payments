using Fiap.Payments.Api.Services.IoC;
using Fiap.Payments.Api.Services.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
  options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
  var generalInfo = new
  {
    Title = "Api - Pagamentos",
    Description = "Api responsável por processar (simular) o pagamento de uma compra de jogo. ",
    Contact = new Microsoft.OpenApi.Models.OpenApiContact
    {
      Name = "Time Fiap",
      Email = "timefiap@fiap.com.br"
    }
  };

  // Definição da Versão 1 
  options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
  {
    Title = generalInfo.Title,
    Version = "v1",
    Description = generalInfo.Description,
    Contact = generalInfo.Contact
  });

  // Definição da Versão 2 
  options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
  {
    Title = generalInfo.Title,
    Version = "v2",
    Description = generalInfo.Description,
    Contact = generalInfo.Contact
  });

  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Insira o token JWT desta forma: Bearer seu_token_aqui"
  });

  options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

#region [JWT] 
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

builder.Services.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
  options.RequireHttpsMetadata = false;
  options.SaveToken = true;
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuer = true,
    ValidateAudience = false,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwtIssuer,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
  };
});

builder.Services.AddAuthorization(options =>
{
  options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

builder.Services.RegisterDependencies(builder.Configuration);
#endregion

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Api - Fiap Games v1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Api - Fiap Games v2");
  });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
