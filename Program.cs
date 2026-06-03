using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebApiTest.Data;
using WebApiTest.Data2;
using WebApiTest.Interface;
using WebApiTest.Repository;
using Microsoft.AspNetCore.Server.Kestrel.Core; // ✅ important pour Kestrel
using Microsoft.AspNetCore.Server.IIS;          // ✅ important pour IIS

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMemoryCache(); // ✅ AJOUT ICI

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowOrigin",
        builder =>
        {
            builder.WithOrigins("*", "https://www.msl-novopharma.tn", "http://localhost:8100", "https://msl-novopharma.tn", "http://localhost:4200")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
        });
});

// ✅ Désactiver la limite de taille de requête
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue; // illimité
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue; // illimité
});

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(x =>
                                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddDbContext<NOVOPHARMAContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddDbContext<MEDSOURCEContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MedsourceConnection"));
});
builder.Services.AddDbContext<PowerAppContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ReclamationConnection"));
});

builder.Services.AddScoped<IMbArticleRepository, MbArticleRepository>();
builder.Services.AddScoped<IMsEnttRepository, MsEnttRepository>();
builder.Services.AddScoped<IClientRepository, MsAClientRepository>();
builder.Services.AddScoped<IReclamationRepository, ReclamationRepository>();
builder.Services.AddScoped<IMs_ContactRepository, MsContactRepository>();
builder.Services.AddScoped<IFa_FamilleRepository, Fa_FamilleRepository>();

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

app.MapControllers();
app.MapFallbackToController("Index", "Fallback");
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthorization();
app.UseCors("AllowOrigin");
app.MapControllers();

await app.RunAsync();
