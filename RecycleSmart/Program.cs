using Microsoft.Extensions.Options;
using Servicio;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddAuthentication(ManejosSesiones.esquema).AddCookie(ManejosSesiones.esquema,
    options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Login/Salir";
        options.AccessDeniedPath = "/Login/AccesoDenegado";
        options.Cookie.Name = "RecycleSmart.Auth";
        options.Cookie.HttpOnly = true;
        options.AccessDeniedPath = "/Home/AccesoDenegado";

    });
builder.Services.AddHostedService<RecycleSmart.Servicios.MotorIoT>();
builder.Services.AddSignalR();
var app=builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<RecycleSmart.HUB.MonitoreoHubs>("/hubs/monitoreo");

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
