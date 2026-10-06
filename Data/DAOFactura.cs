using MongoDB.Driver;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOFactura
{
    private readonly IMongoCollection<FacturaDoc> _facturas = Conexion.GetCollection<FacturaDoc>("facturas");
    private readonly IMongoCollection<PedidoDoc> _pedidos = Conexion.GetCollection<PedidoDoc>("pedidos");
    private readonly IMongoCollection<UsuarioDoc> _usuarios = Conexion.GetCollection<UsuarioDoc>("usuarios");

    public int GenerarFactura(int idPedido, List<DetallePedido> detalles)
    {
        double subtotal = detalles.Sum(d => d.PrecioUnitario * d.Cantidad);
        double cargoServicio = subtotal * 0.10;
        double iva = subtotal * 0.12;
        double total = subtotal + cargoServicio + iva;

        var doc = new FacturaDoc
        {
            Id = Conexion.SiguienteId("facturas"),
            IdPedido = idPedido,
            Subtotal = subtotal,
            CargoServicio = cargoServicio,
            Iva = iva,
            Total = total,
            Estado = "EMITIDA",
            Fecha = DateTime.Now
        };
        _facturas.InsertOne(doc);
        return doc.Id;
    }

    public Factura? BuscarPorId(int id)
    {
        var doc = _facturas.Find(f => f.Id == id).FirstOrDefault();
        return doc == null ? null : Mapear(doc);
    }

    public Factura? BuscarPorPedido(int idPedido)
    {
        var doc = _facturas.Find(f => f.IdPedido == idPedido).FirstOrDefault();
        return doc == null ? null : Mapear(doc);
    }

    public List<Factura> Listar() =>
        _facturas.Find(_ => true).ToList().OrderByDescending(f => f.Fecha).Select(Mapear).ToList();

    public Dictionary<string, double> EstadisticasHoy()
    {
        var hoy = DateTime.Today;
        var facturasHoy = _facturas.Find(f => f.Fecha >= hoy && f.Fecha < hoy.AddDays(1) && f.Estado == "EMITIDA").ToList();
        return new Dictionary<string, double>
        {
            ["pedidos"] = facturasHoy.Count,
            ["ventas"] = facturasHoy.Sum(f => f.Total)
        };
    }

    private Factura Mapear(FacturaDoc d)
    {
        var pedido = _pedidos.Find(p => p.Id == d.IdPedido).FirstOrDefault();
        var cliente = pedido?.IdUsuario != null ? _usuarios.Find(u => u.Id == pedido.IdUsuario).FirstOrDefault() : null;
        var mesero = pedido?.IdMesero != null ? _usuarios.Find(u => u.Id == pedido.IdMesero).FirstOrDefault() : null;
        return new Factura
        {
            IdFactura = d.Id,
            IdPedido = d.IdPedido,
            Fecha = d.Fecha.ToString("O"),
            Subtotal = d.Subtotal,
            CargoServicio = d.CargoServicio,
            Iva = d.Iva,
            Total = d.Total,
            Estado = d.Estado,
            ClienteNombre = $"{cliente?.Nombre ?? "Invitado"} {cliente?.Apellido ?? ""}".Trim(),
            MeseroNombre = $"{mesero?.Nombre ?? ""} {mesero?.Apellido ?? ""}".Trim()
        };
    }
}
