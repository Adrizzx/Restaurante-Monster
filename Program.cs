using RestauranteMonster.Data;
using RestauranteMonster.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RestauranteMonster.Session";
});

var app = builder.Build();

Conexion.Configure(
    builder.Configuration.GetConnectionString("MongoConnection")!,
    builder.Configuration["MongoDatabase"]!);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Seguridad/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Soporte para AVIF y WEBP como tipos estáticos
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".avif"] = "image/avif";
provider.Mappings[".webp"] = "image/webp";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Seguridad}/{action=Login}/{id?}");

app.Run();
