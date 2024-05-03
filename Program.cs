using APICatalogo.Context;
using APICatalogo.DTOs.Mapping;
using APICatalogo.Extensions;
using APICatalogo.Filters;
using APICatalogo.Logging;
using APICatalogo.Models;
using APICatalogo.Repositories;
using APICatalogo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(ApiExceptionFilter)); // Adiciona o filtro ApiExceptionFilter
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // Ignora refer�ncias c�clicas ao serializar objetos.
})
.AddNewtonsoftJson();

// var OrigensComAcessoPermitido = "_origensComAcessoPermitido"; // Define o nome da pol�tica de CORS

builder.Services.AddCors(options => {
    options.AddPolicy(name: "OrigensComAcessoPermitido", // Define o nome da pol�tica de CORS
    policy => {
        policy.WithOrigins("https://localhost:7030")
        .WithMethods("GET", "POST")
        .AllowAnyHeader(); // Define as origens com acesso permitido
    });
});

var mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection"); // Pega a string de conex�o do appsettings.json

var valor1 = builder.Configuration["chave1"];

Console.WriteLine($"Valor1: {valor1}");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( c => {
    c.SwaggerDoc("v1", new() { Title = "APICatalogo", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

builder.Services.AddAuthorization();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>() // Configura o Identity
    .AddEntityFrameworkStores<AppDbContext>() // Configura o Entity Framework como o armazenamento do Identity
    .AddDefaultTokenProviders(); // Adiciona o provedor de token padr�o

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(mySqlConnection, ServerVersion.AutoDetect(mySqlConnection))); // Configura o DbContext com o MySQL

var secretKey = builder.Configuration["JWT:SecretKey"] ?? throw new ArgumentException("Invalid secret key!!"); // Pega a chave secreta do appsettings.json

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Define o esquema de autentica��o padr�o
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Define o esquema de desafio padr�o
}).AddJwtBearer(options =>
{
    options.SaveToken = true; // Salva o token de acesso
    options.RequireHttpsMetadata = false; // N�o requer HTTPS, em produção deve ser true
    options.TokenValidationParameters = new TokenValidationParameters() // Config do token
    {
        ValidateIssuer = true, // Valida o emissor
        ValidateAudience = true, // Valida o destinat�rio
        ValidateLifetime = true, // Valida o tempo de vida
        ValidateIssuerSigningKey = true, // Valida a chave de assinatura
        ClockSkew = TimeSpan.Zero, // Clock skew, que é o tempo máximo permitido entre a hora de expiração do token e a hora atual
        ValidAudience = builder.Configuration["JWT:ValidAudience"], // Audi�ncia v�lida
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"], // Emissor v�lido
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)) // Chave de assinatura
    };
});

builder.Services.AddAuthorization(options => {
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin")); // Pol�tica de autoriza��o
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("Admin").RequireClaim("id", "joaopedro"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
    options.AddPolicy("ExclusivePolicyOnly", policy => policy.RequireAssertion(context => 
        context.User.HasClaim(c => c.Type == "id" && c.Value == "joaopedro") || context.User.IsInRole("SuperAdmin")
    ));
});

builder.Services.AddTransient<IMeuServico, MeuServico>(); // Registra o servi�o MeuServico no container de inje��o de depend�ncia
builder.Services.AddScoped<ApiLoggingFilter>(); // Registra o filtro ApiLoggingFilter no container de inje��o de depend�ncia
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // Registra o repositório genérico no container de injeção de dependência
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddAutoMapper(typeof(ProdutoDTOMappingProfile)); // Configura o AutoMapper
builder.Services.AddScoped<ITokenService, TokenService>(); // Registra o servi�o TokenService no container de inje��o de depend�ncia

builder.Logging.AddProvider(new CustomLoggerProvider(new CustomLoggerProviderConfiguration
{ // Adiciona o provedor de log personalizado
    LogLevel = LogLevel.Information,
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ConfigureExceptionHandler(); // Middleware de tratamento de exce��es
}

app.UseHttpsRedirection(); // Middleware de redirecionamento HTTPS
app.UseStaticFiles(); // Middleware de arquivos est�ticos
app.UseRouting(); // Middleware de roteamento

// app.UseAuthentication();

// app.UseCors(OrigensComAcessoPermitido); // Define a pol�tica de CORS
app.UseCors();

app.UseAuthorization(); // Middleware de autoriza��o

app.Use(async (context, next) => // Request delegate. 
{
    // Request
    await next(context); // Chama o pr�ximo middleware
    // Response
});

app.MapControllers();

//app.Run(async (context) =>
//{
//    await context.Response.WriteAsync("Middlware final (de dentro do Run)"); // Middleware final
//});

app.Run();
