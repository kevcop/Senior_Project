using Microsoft.EntityFrameworkCore;
using Senior_Project.Models;
using Senior_Project.Data;

var builder = WebApplication.CreateBuilder(args);

// Register your DbContexts
builder.Services.AddDbContext<New_Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("New_Context") ?? throw new InvalidOperationException("Connection string 'New_Context' not found.")));
builder.Services.AddDbContext<Context_file2>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Context_file2") ?? throw new InvalidOperationException("Connection string 'Context_file2' not found.")));

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

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<New_Context>();
        DataSeeder.Seed(context); // Call your seeding logic
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
    }
}

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

app.Run();
