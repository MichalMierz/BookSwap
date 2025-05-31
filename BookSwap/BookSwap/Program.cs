using BookSwap.Data;
using BookSwap.Models;
using BookSwap.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Dodaj DbContext (SQL Server). Upewnij się, że w appsettings.json jest sekcja "ConnectionStrings:DefaultConnection".
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Dodaj tożsamość (Identity) z naszymi klasami ApplicationUser i IdentityRole.
//    Usuwamy tutaj AddDefaultIdentity<…>() i zostawiamy tylko AddIdentity<…>().
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Tu możesz dostosować wymagania dotyczące haseł, lockout itp.
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

//Rejestracja RoleSeeder
builder.Services.AddScoped<IRoleSeeder, RoleSeeder>();

// 3. Dodaj MVC i Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 4. (Opcjonalnie) Seedowanie ról przy starcie
builder.Services.AddScoped<IRoleSeeder, RoleSeeder>();

var app = builder.Build();

// 5. Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Ważne: tylko jedno UseAuthentication()
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IRoleSeeder>();
    await seeder.SeedRolesAsync();
}

// 6. Domyślna trasa MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // potrzebne, żeby obsłużyć wygenerowane strony Identity

// 7. Seedowanie ról w bazie
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IRoleSeeder>();
    await seeder.SeedRolesAsync();
}

app.Run();
