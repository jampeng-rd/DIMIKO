using ECommerce.Business.Services;
using ECommerce.Business.Services.IServices;
using ECommerce.DataAccess.Data;
using ECommerce.Models;
using ECommerce.Utility.Identity;
using ECommerce.Utility.Settings;
using ECommerce.Web.BackgroundServices;
using ECommerce.Web.DataInitialization;
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

// Register NewebPay settings (註冊藍新金流設定)
builder.Services.Configure<NewebPaySettings>(builder.Configuration.GetSection("NewebPay"));

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
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IHeroBannerService, HeroBannerService>();
// Register NewebPay service
builder.Services.AddScoped<INewebPayService, NewebPayService>();

builder.Services.AddHostedService<ExpiredOrderCleanupService>();

// Register Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

// 設定 Identity 登入與拒絕存取路徑
builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/Identity/Account/login";
	options.LogoutPath = "/Identity/Account/Logout";
	options.AccessDeniedPath = "/Identity/Account/AccessDenied";
	options.ExpireTimeSpan = TimeSpan.FromDays(1);
});
// 最多約 5 分鐘後重新驗證安全戳記
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
	options.ValidationInterval = TimeSpan.FromMinutes(5);
});


var app = builder.Build();


// Initialize Identity roles and initial administrator. 初始化建立管理員
using (var scope = app.Services.CreateScope())
{
	await IdentityInitializer.InitializeAsync(scope.ServiceProvider, app.Configuration);
}

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

app.UseSession();

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
