using MongoDB.Driver;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOPedido
{
    private readonly IMongoCollection<PedidoDoc> _pedidos = Conexion.GetCollection<PedidoDoc>("pedidos");
    private readonly IMongoCollection<UsuarioDoc> _usuarios = Conexion.GetCollection<UsuarioDoc>("usuarios");
    private readonly IMongoCollection<PlatoDoc> _platos = Conexion.GetCollection<PlatoDoc>("platos");
    private readonly IMongoCollection<BebidaDoc> _bebidas = Conexion.GetCollection<BebidaDoc>("bebidas");

    public int Crear(int idUsuario, int idMesero, string tipoPedido)
    {
        var doc = new PedidoDoc
        {
            Id = Conexion.SiguienteId("pedidos"),
            IdUsuario = idUsuario > 0 ? idUsuario : null,
            IdMesero = idMesero > 0 ? idMesero : null,
            TipoPedido = tipoPedido,
            Estado = "PENDIENTE",
            Fecha = DateTime.Now
        };
        _pedidos.InsertOne(doc);
        return doc.Id;
    }

    public void AgregarDetalle(DetallePedido d)
    {
        var detalle = new DetalleDoc
        {
            IdDetalle = Conexion.SiguienteId("detalles_pedido"),
            IdPlato = d.IdPlato > 0 ? d.IdPlato : null,
            IdBebida = d.IdBebida > 0 ? d.IdBebida : null,
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            Subtotal = d.PrecioUnitario * d.Cantidad
        };
        _pedidos.UpdateOne(p => p.Id == d.IdPedido, Builders<PedidoDoc>.Update.Push(p => p.Detalles, detalle));
    }

    public void ActualizarEstado(int idPedido, string estado)
    {
        _pedidos.UpdateOne(p => p.Id == idPedido, Builders<PedidoDoc>.Update.Set(p => p.Estado, estado));
    }

    public Pedido? BuscarPorId(int id)
    {
        var doc = _pedidos.Find(p => p.Id == id).FirstOrDefault();
        return doc == null ? null : Mapear(doc);
    }

    public List<Pedido> ListarTodos() =>
        _pedidos.Find(_ => true).ToList().OrderByDescending(p => p.Fecha).Select(Mapear).ToList();

    public List<Pedido> ListarPorMesero(int idMesero)
    {
        var hoy = DateTime.Today;
        return _pedidos.Find(p => p.IdMesero == idMesero && p.Fecha >= hoy && p.Fecha < hoy.AddDays(1))
            .ToList().OrderByDescending(p => p.Fecha).Select(Mapear).ToList();
    }

    public List<DetallePedido> ObtenerDetalles(int idPedido)
    {
        var doc = _pedidos.Find(p => p.Id == idPedido).FirstOrDefault();
        return doc == null ? new List<DetallePedido>() : MapearDetalles(doc.Detalles);
    }

    private List<DetallePedido> MapearDetalles(List<DetalleDoc> detalles) =>
        detalles.Select(dd =>
        {
            string? nombre = null, imagen = null;
            var tipoItem = "BEBIDA";
            if (dd.IdPlato.HasValue)
            {
                var plato = _platos.Find(p => p.Id == dd.IdPlato).FirstOrDefault();
                nombre = plato?.Nombre;
                imagen = plato?.Imagen;
                tipoItem = "PLATO";
            }
            else if (dd.IdBebida.HasValue)
            {
                var bebida = _bebidas.Find(b => b.Id == dd.IdBebida).FirstOrDefault();
                nombre = bebida?.Nombre;
                imagen = bebida?.Imagen;
            }
            return new DetallePedido
            {
                IdDetalle = dd.IdDetalle,
                IdPedido = 0,
                IdPlato = dd.IdPlato ?? 0,
                IdBebida = dd.IdBebida ?? 0,
                Cantidad = dd.Cantidad,
                PrecioUnitario = dd.PrecioUnitario,
                Subtotal = dd.Subtotal,
                NombreItem = nombre ?? "",
                Imagen = imagen,
                TipoItem = tipoItem
            };
        }).ToList();

    private Pedido Mapear(PedidoDoc d)
    {
        var cliente = d.IdUsuario.HasValue ? _usuarios.Find(u => u.Id == d.IdUsuario).FirstOrDefault() : null;
        var mesero = d.IdMesero.HasValue ? _usuarios.Find(u => u.Id == d.IdMesero).FirstOrDefault() : null;
        var detalles = MapearDetalles(d.Detalles);
        foreach (var det in detalles) det.IdPedido = d.Id;
        return new Pedido
        {
            IdPedido = d.Id,
            IdUsuario = d.IdUsuario ?? 0,
            IdMesero = d.IdMesero ?? 0,
            Fecha = d.Fecha.ToString("O"),
            Estado = d.Estado,
            TipoPedido = d.TipoPedido,
            ClienteNombre = $"{cliente?.Nombre ?? "Invitado"} {cliente?.Apellido ?? ""}".Trim(),
            MeseroNombre = $"{mesero?.Nombre ?? ""} {mesero?.Apellido ?? ""}".Trim(),
            Detalles = detalles
        };
    }
}
