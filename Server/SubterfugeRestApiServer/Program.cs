using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using SubterfugeRestApiServer.Authentication;
using SubterfugeRestApiServer.Database;
using SubterfugeRestApiServer.Middleware;
using SubterfugeServerConsole.Connections;
using HostingEnvironmentExtensions = Microsoft.AspNetCore.Hosting.HostingEnvironmentExtensions;

var builder = WebApplication.CreateBuilder(args);

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// Add services to the container.

// Add Exception Handling Filter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExceptionResponseMiddleware>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    }
);

builder.Services.AddSwaggerGen(genOptions =>
    {
        genOptions.SwaggerDoc("v1", new OpenApiInfo { Title = "Subterfuge Community Edition API", Version = "v1" });

        // Include 'SecurityScheme' to use JWT Authentication
        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            BearerFormat = "JWT",
            Name = "JWT Authentication",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

            Reference = new OpenApiReference
            {
                Id = JwtBearerDefaults.AuthenticationScheme,
                Type = ReferenceType.SecurityScheme
            }
        };

        genOptions.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

        genOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { jwtSecurityScheme, Array.Empty<string>() }
        });
    }
);

// Configure Logging
builder.Services.AddLogging(logging => logging.AddSimpleConsole(loggerOptions =>
{
    loggerOptions.SingleLine = true;
    loggerOptions.IncludeScopes = false;
    loggerOptions.TimestampFormat = "[yyyy-MM-dd HH:mm:ss]";
}));

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

// Enable JWT Authentication.
new JwtAuthenticationScheme(config).ConfigureAuthentication(builder.Services);

// Add Database as a singleton to Dependency Inject.
// MongoConfiguration mongoConfig = new MongoConfiguration(app.Configuration.GetSection("MongoDb"));
// MongoConnector mongo = new MongoConnector(mongoConfig, builder.Logging.);

builder.Services.AddCors(options =>
{
    options.AddPolicy("SubterfugePolicy", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddSingleton<IMongoConfigurationProvider, MongoConfigProvider>();
builder.Services.AddSingleton<IDatabaseCollectionProvider, MongoConnector>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("SubterfugePolicy");

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// ORDER MATTERS HERE
app.UseAuthorization();
app.UseMiddleware<JwtMiddleware>();
app.UseMiddleware<BanMiddleware>();
app.MapControllers();
app.UseMiddleware<LoggingMiddleware>();

app.Run();
