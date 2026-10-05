using ListingSearch.Api.Data;
using ListingSearch.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency injection
builder.Services.AddSingleton<IListingRepository>(sp =>
{
    var environment = sp.GetRequiredService<IWebHostEnvironment>();

    var filePath = Path.Combine(
        environment.ContentRootPath,
        "Data",
        "sample_listings.json");

    return new InMemoryListingRepository(filePath);
});

builder.Services.AddSingleton<IClock, SystemClock>();

builder.Services.AddScoped<IListingSearchService, ListingSearchService>();

// Allow Angular development server to call the API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AngularClient");

app.MapControllers();

app.Run();