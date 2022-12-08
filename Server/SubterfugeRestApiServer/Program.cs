using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using SubterfugeRestApiServer;
using SubterfugeRestApiServer.Authentication;
using SubterfugeServerConsole.Connections;

System.Diagnostics.Debug.WriteLine("Uhh what?");
var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
);

builder.Services.AddSwaggerGen(genOptions =>
    {
        genOptions.SwaggerDoc("v1", new OpenApiInfo { Title = "Subterfuge Community Edition API", Version = "v1" });

        // genOptions.SchemaFilter<EnumSchemaFilter>();

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

// Enable JWT Authentication.
new JwtAuthenticationScheme(config).ConfigureAuthentication(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    app.UseHttpLogging();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

/**
 * For Docker:
 * Host: "db"
 * Port: 27017
 *
 * For local:
 * Host: "localhost"
 * Port: 27017
 */
System.Diagnostics.Debug.WriteLine("Linking Mongo...");
MongoConnector mongo = new MongoConnector(config["MongoDb:Host"], Convert.ToInt32(config["MongoDb:Port"]), true);

System.Diagnostics.Debug.WriteLine("Starting App...");
app.Run();
