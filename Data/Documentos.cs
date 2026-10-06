using MongoDB.Bson.Serialization.Attributes;

namespace RestauranteMonster.Data;

// Documentos internos de persistencia en MongoDB (colecciones). Separados de los Models
// que usan las Views/Controllers para no acoplar la capa de presentación al esquema de Mongo.

[BsonIgnoreExtraElements]
public class RolDoc
{
    [BsonId] public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

[BsonIgnoreExtraElements]
public class UsuarioDoc
{
    [BsonId] public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public int IdRol { get; set; }
    public int Estado { get; set; } = 1;
}

[BsonIgnoreExtraElements]
public class TipoPlatoDoc
{
    [BsonId] public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

[BsonIgnoreExtraElements]
public class PlatoDoc
{
    [BsonId] public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public string? Imagen { get; set; }
    public int IdTipo { get; set; }
    public bool Disponible { get; set; } = true;
}

[BsonIgnoreExtraElements]
public class BebidaDoc
{
    [BsonId] public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public string? Imagen { get; set; }
    public bool Disponible { get; set; } = true;
}

[BsonIgnoreExtraElements]
public class StockDoc
{
    [BsonId] public int Id { get; set; }
    public int? IdPlato { get; set; }
    public int? IdBebida { get; set; }
    public int Cantidad { get; set; }
    public int StockMinimo { get; set; } = 5;
}

[BsonIgnoreExtraElements]
public class DetalleDoc
{
    public int IdDetalle { get; set; }
    public int? IdPlato { get; set; }
    public int? IdBebida { get; set; }
    public int Cantidad { get; set; }
    public double PrecioUnitario { get; set; }
    public double Subtotal { get; set; }
}

[BsonIgnoreExtraElements]
public class PedidoDoc
{
    [BsonId] public int Id { get; set; }
    public int? IdUsuario { get; set; }
    public int? IdMesero { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public string Estado { get; set; } = "PENDIENTE";
    public string TipoPedido { get; set; } = "MESA";
    public List<DetalleDoc> Detalles { get; set; } = new();
}

[BsonIgnoreExtraElements]
public class FacturaDoc
{
    [BsonId] public int Id { get; set; }
    public int IdPedido { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public double Subtotal { get; set; }
    public double CargoServicio { get; set; }
    public double Iva { get; set; }
    public double Total { get; set; }
    public string Estado { get; set; } = "EMITIDA";
}

[BsonIgnoreExtraElements]
public class ClienteDoc
{
    [BsonId] public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string? Direccion { get; set; }
    public string? FechaNacimiento { get; set; }
}
