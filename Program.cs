using Microsoft.EntityFrameworkCore;
using Senior_Project.Models;
using Senior_Project.Data;
using Microsoft.Extensions.DependencyInjection;
using Senior_Project.SignalR;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

// Register your DbContexts
builder.Services.AddDbContext<NewContext2>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NewContext2") ?? throw new InvalidOperationException("Connection string 'Context_file2' not found.")));


// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.Initialize(services); // Await if the method is asynchronous
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred seeding the DB: {ex.Message}");
    }
}

// Seed the database

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<UserMessaging>("/userMessaging");

app.Run();
