using Amazon.S3;
using LimpezaDeBase.Infra;
using LimpezaDeBase.Infra.Interfaces;
using LimpezaDeBase.Infra.Repository;
using LimpezaDeBase.Limpeza;
using LimpezaDeBase.Limpeza.Interfaces;
using LimpezaDeBase.Services;
using LimpezaDeBase.Services.Interfaces;
using MongoDB.Driver;
using RestEase;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

builder.Logging.ClearProviders();
builder.Logging.AddLambdaLogger();

builder.Services.AddScoped<IMongoClient>(serviceProvider => new MongoClient("mongodb+srv://admin:flowchat123456@cluster0.dwcxd.mongodb.net"));
builder.Services.AddScoped(rest => RestClient.For<IOtimaAPI>("https://apivalida.otima.digital/"));

builder.Services.AddScoped<IMongoService, MongoService>();
builder.Services.AddScoped<ILimpezaService, LimpezaService>();
builder.Services.AddScoped<IBlipContatoService, BlipContatoService>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IOptOutService, OptOutService>();
builder.Services.AddScoped<ICsvService, CsvService>();
builder.Services.AddScoped<IProcessamentoService, ProcessamentoService>();
builder.Services.AddScoped<INotificacaoService, NoticacaoService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();

builder.Services.AddScoped<LimpezaFactory>();

builder.Services.AddScoped<ITelefoneRepository>(provider => new TelefoneRepository("Host=skepsdata.postgres.database.azure.com;Database=disparador;Username=christian;Password=skeps@123"));
builder.Services.AddScoped<IProcessamentoRepository>(provider => new ProcessamentoRepository("Host=skepsdata.postgres.database.azure.com;Database=disparador;Username=christian;Password=skeps@123"));
builder.Services.AddScoped<IRelatorioRepository>(provider => new RelatorioRepository("Host=skepsdata.postgres.database.azure.com;Database=disparador;Username=christian;Password=skeps@123"));

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<IEmail, Email>();
builder.Services.AddScoped<IBlip, Blip>();
builder.Services.AddScoped<IAwsS3, AwsS3>();

builder.Services.AddScoped(c => new Dictionary<string, string>
                {
                    {"client_id","120414059866b608fe0a386981767296"},
                    {"password", "Skeps##1501"},
                    {"username","alan@skeps.com.br" }
                });

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

var app = builder.Build();
app.UseCors("AllowAll");

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Welcome to running ASP.NET Core Minimal API on AWS Lambda");

app.Run();
