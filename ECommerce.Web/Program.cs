using ECommerce.Business.Services;
using ECommerce.Business.Services.IServices;
using ECommerce.DataAccess.Data;
using ECommerce.Models;
using ECommerce.Utility.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("SQLConnection"));
});


// Register Identity
builder.Services
	.AddIdentity<ApplicationUser, IdentityRole>(options =>
	{
		options.User.RequireUniqueEmail = true;

		options.Password.RequireDigit = true;              // 密碼至少需要包含一個數字（0-9）
		options.Password.RequireLowercase = true;          // 密碼至少需要包含一個英文小寫字母（a-z）。
		options.Password.RequireUppercase = true;          // 密碼至少需要包含一個英文大寫字母（A-Z）。
		options.Password.RequireNonAlphanumeric = true;    // 密碼至少需要包含一個特殊符號。
		options.Password.RequiredLength = 7;               // 密碼至少需要 7 個字元。
		options.Password.RequiredUniqueChars = 4;          // 密碼至少需要包含 4 個不同字元。
	})
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddErrorDescriber<ChineseIdentityErrorDescriber>();

// Register Repository Pattern
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductImageFileService, ProductImageFileService>();


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

app.MapStaticAssets();

app.MapControllerRoute(
	name: "MyArea",
	pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}",
	defaults: new { area = "Customer" })
	.WithStaticAssets();

app.Run();
