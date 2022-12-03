using SubterfugeServerConsole.Connections;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Setup mongoDB
const String Hostname = "server"; // For docker
// private const String Hostname = "localhost"; // For local
const int Port = 5000;
        
const String dbHost = "db"; // For docker
// private const String dbHost = "localhost"; // For local
const int dbPort = 27017;

MongoConnector mongo = new MongoConnector(dbHost, dbPort, false);

app.Run();
