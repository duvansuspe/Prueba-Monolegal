using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Monolegal.Web.Data;

public class MongoDbContext
{
    public MongoDbContext(IOptions<OpcionesMongo> opciones)
    {
        var configuracion = opciones.Value;
        var cliente = new MongoClient(configuracion.ConnectionString);
        Database = cliente.GetDatabase(configuracion.DatabaseName);
        Clientes = Database.GetCollection<Cliente>("clientes");
        Facturas = Database.GetCollection<Factura>("facturas");
        Notificaciones = Database.GetCollection<NotificacionCorreo>("notificaciones");
    }

    public IMongoDatabase Database { get; }
    public IMongoCollection<Cliente> Clientes { get; }
    public IMongoCollection<Factura> Facturas { get; }
    public IMongoCollection<NotificacionCorreo> Notificaciones { get; }
}
