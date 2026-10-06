using MongoDB.Driver;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOBebida
{
    private readonly IMongoCollection<BebidaDoc> _bebidas = Conexion.GetCollection<BebidaDoc>("bebidas");
    private readonly IMongoCollection<StockDoc> _stocks = Conexion.GetCollection<StockDoc>("stocks");

    public List<Bebida> Listar() =>
        _bebidas.Find(_ => true).ToList().OrderBy(b => b.Nombre).Select(Mapear).ToList();

    public List<Bebida> ListarDisponibles() =>
        _bebidas.Find(b => b.Disponible).ToList().OrderBy(b => b.Nombre).Select(Mapear).ToList();

    public Bebida? BuscarPorId(int id)
    {
        var d = _bebidas.Find(b => b.Id == id).FirstOrDefault();
        return d == null ? null : Mapear(d);
    }

    public void Insertar(Bebida b)
    {
        var doc = new BebidaDoc
        {
            Id = Conexion.SiguienteId("bebidas"),
            Nombre = b.Nombre,
            Descripcion = b.Descripcion,
            Precio = b.Precio,
            Imagen = b.Imagen,
            Disponible = true
        };
        _bebidas.InsertOne(doc);
        _stocks.InsertOne(new StockDoc { Id = Conexion.SiguienteId("stocks"), IdBebida = doc.Id, Cantidad = 80, StockMinimo = 10 });
    }

    public void Actualizar(Bebida b)
    {
        var update = Builders<BebidaDoc>.Update
            .Set(x => x.Nombre, b.Nombre)
            .Set(x => x.Descripcion, b.Descripcion)
            .Set(x => x.Precio, b.Precio);
        if (b.Imagen != null) update = update.Set(x => x.Imagen, b.Imagen);
        _bebidas.UpdateOne(x => x.Id == b.IdBebida, update);
    }

    public void CambiarDisponibilidad(int id, bool disponible)
    {
        _bebidas.UpdateOne(x => x.Id == id, Builders<BebidaDoc>.Update.Set(x => x.Disponible, disponible));
    }

    private Bebida Mapear(BebidaDoc d) => new()
    {
        IdBebida = d.Id,
        Nombre = d.Nombre,
        Descripcion = d.Descripcion,
        Precio = d.Precio,
        Imagen = d.Imagen,
        Disponible = d.Disponible,
        Stock = _stocks.Find(s => s.IdBebida == d.Id).FirstOrDefault()?.Cantidad ?? 0
    };
}
