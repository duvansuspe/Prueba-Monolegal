using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Monolegal.Web.Models;

public class Factura
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("clienteId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ClienteId { get; set; } = string.Empty;

    [BsonElement("numero")]
    public string Numero { get; set; } = string.Empty;

    [BsonElement("concepto")]
    public string Concepto { get; set; } = string.Empty;

    [BsonElement("valor")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Valor { get; set; }

    [BsonElement("fechaEmision")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaEmision { get; set; }

    [BsonElement("fechaVencimiento")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaVencimiento { get; set; }

    [BsonElement("estado")]
    public string Estado { get; set; } = string.Empty;
}
