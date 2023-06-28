using Book_Seller_Website.Data.Context;
using Book_Seller_Website.Models.Interface;
using Book_Seller_Website.Models.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Book_Seller_Website.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using Book_Seller_Website.Models.DbInitializer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BookSellerDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// options => options.SignIn.RequireConfirmedAccount = true phai verify email moi login duoc
// AddIdentity<IdentityUser, IdentityRole> hoac .AddIdentityCore<ApiUser>().AddRoles<IdentityRole>
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<BookSellerDbContext>().AddDefaultTokenProviders();
builder.Services.AddRazorPages();

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// override default access deny url, must add after addidentity
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
}); // add session khi dung stripe

builder.Services.AddAuthentication().AddFacebook(option => {
    option.AppId = "969194751063538";
    option.AppSecret = "0259f06f119a16850f14adea924cf1cd";
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

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

StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

SeedDatabase();

app.UseSession(); // add session khi dung stripe

app.MapRazorPages(); // identity razor page

app.MapControllerRoute( // phai map truoc default route 
    name: "Admin",
    pattern: "{area:exists}/{controller=Categories}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
    );


app.Run();

void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}
