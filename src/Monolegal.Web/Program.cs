using Monolegal.Web.Data;
using Monolegal.Web.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<OpcionesMongo>()
    .Bind(builder.Configuration.GetSection("MongoDb"))
    .Validate(opciones => !string.IsNullOrWhiteSpace(opciones.ConnectionString), "Configure MongoDb:ConnectionString.")
    .Validate(opciones => !string.IsNullOrWhiteSpace(opciones.DatabaseName), "Configure MongoDb:DatabaseName.")
    .ValidateOnStart();

builder.Services.AddOptions<OpcionesSmtp>()
    .Bind(builder.Configuration.GetSection("Smtp"))
    .Validate(opciones => !string.IsNullOrWhiteSpace(opciones.Host), "Configure Smtp:Host.")
    .Validate(opciones => opciones.Port > 0, "Configure Smtp:Port.")
    .ValidateOnStart();

builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IFacturaRepositorio, FacturaRepositorio>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IRecordatorioService, RecordatorioService>();
builder.Services.AddScoped<IDatosIniciales, DatosInicialesService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

await using (var alcance = app.Services.CreateAsyncScope())
{
    var datos = alcance.ServiceProvider.GetRequiredService<IDatosIniciales>();
    var bitacora = alcance.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Inicio");
    try
    {
        await datos.AsegurarAsync(CancellationToken.None);
        bitacora.LogInformation("MongoDB listo. Los datos de demostración se crean solo si la base está vacía.");
    }
    catch (Exception excepcion)
    {
        bitacora.LogCritical(excepcion, "No fue posible conectar con MongoDB. Verifique que el servidor esté en ejecución.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Facturas/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Facturas}/{action=Index}/{id?}");

app.Run();
