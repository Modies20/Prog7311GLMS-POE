using GLMS.Services;  // For IApiService, ApiService
using GLMS.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();


// REMOVED: Direct DbContext (now API handles database)
// builder.Services.AddDbContext<ApplicationDbContext>(...)


// ADDED: Session support (for storing auth token)

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// ADDED: HttpClient for calling the API

builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    // Read API base URL from configuration (default to Docker service name)
    var apiUrl = builder.Configuration["ApiUrl"] ?? "http://glms-backend-api:8080/";
    client.BaseAddress = new Uri(apiUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register HttpContextAccessor (so ApiService can access session)
builder.Services.AddHttpContextAccessor();


// Keep UI?only services (no database access)

builder.Services.AddScoped<ICurrencyExchangeService, CurrencyExchangeService>();
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddScoped<IContractValidationService, ContractValidationService>();

// Add response caching for better performance
builder.Services.AddResponseCaching();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


// ADDED: Session middleware (MUST be before Authorization)

app.UseSession();

app.UseResponseCaching();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// REMOVED: Database creation – now handled by the API container

// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     dbContext.Database.EnsureCreated();
// }

app.Run();