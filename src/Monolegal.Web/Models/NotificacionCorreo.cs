using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Monolegal.Web.Models;

public class NotificacionCorreo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("clienteId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ClienteId { get; set; } = string.Empty;

    [BsonElement("clienteNombre")]
    public string ClienteNombre { get; set; } = string.Empty;

    [BsonElement("facturaId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FacturaId { get; set; } = string.Empty;

    [BsonElement("numeroFactura")]
    public string NumeroFactura { get; set; } = string.Empty;

    [BsonElement("destinatario")]
    public string Destinatario { get; set; } = string.Empty;

    [BsonElement("asunto")]
    public string Asunto { get; set; } = string.Empty;

    [BsonElement("mensaje")]
    public string Mensaje { get; set; } = string.Empty;

    [BsonElement("estadoAnterior")]
    public string EstadoAnterior { get; set; } = string.Empty;

    [BsonElement("estadoNuevo")]
    public string EstadoNuevo { get; set; } = string.Empty;

    [BsonElement("fechaEnvio")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaEnvio { get; set; }
}
