using Book_Seller_Website.Data.Context;
using Book_Seller_Website.Models.Interface;
using Book_Seller_Website.Models.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Book_Seller_Website.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BookSellerDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// options => options.SignIn.RequireConfirmedAccount = true phai verify email moi login duoc
// AddIdentity<IdentityUser, IdentityRole> hoac .AddIdentityCore<ApiUser>().AddRoles<IdentityRole>
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<BookSellerDbContext>().AddDefaultTokenProviders();
builder.Services.AddRazorPages();

// override default access deny url, must add after addidentity
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
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

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages(); // identity razor page

app.MapControllerRoute( // phai map truoc default route 
    name: "Admin",
    pattern: "{area:exists}/{controller=Categories}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
    );


app.Run();
