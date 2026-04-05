
using Jrq.DataAccess.Data;
using Jrq.DataAccess.Repository;
using Jrq.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Jrq.Utility;
using Stripe;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDBContext>(options=>       
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

builder.Services.AddIdentity<IdentityUser,IdentityRole>().AddEntityFrameworkStores<ApplicationDBContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
// Resolve Stripe secret from configuration or environment (support user-secrets / env vars)
var configuration = app.Configuration;
var stripeKey = configuration["Stripe:SecretKey"] ?? Environment.GetEnvironmentVariable("Stripe__SecretKey");
if (string.IsNullOrWhiteSpace(stripeKey))
{
    // Log a warning in case the secret is missing to avoid runtime errors; payment features will be disabled.
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning("Stripe secret key not found. Set 'Stripe:SecretKey' via user-secrets or the environment variable 'Stripe__SecretKey'. Payment functionality will be disabled.");
}
else
{
    StripeConfiguration.ApiKey = stripeKey;
}
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
