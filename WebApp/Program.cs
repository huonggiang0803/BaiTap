using WebApp.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<IMasterPorductService, MasterProductService>();
builder.Services.AddHttpClient<ISaleOutService, SaleOutService>();
builder.Services.AddControllersWithViews()
    .AddViewOptions(options =>
    {
        options.HtmlHelperOptions.ClientValidationEnabled = true;
    });

builder.Services.AddHttpClient();
builder.Services.AddScoped<IMasterPorductService, MasterProductService>();
builder.Services.AddScoped<ISaleOutService, SaleOutService>();
builder.Services.AddHttpClient("MyApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:44367/"); // API URL
});
builder.Services.AddHttpClient<IMasterPorductService, MasterProductService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:44367/api/");
});

builder.Services.AddScoped<ISaleOutService, SaleOutService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Hiển thị chi tiết lỗi
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=MasterProduct}/{action=Index}/{id?}");

app.Run();
