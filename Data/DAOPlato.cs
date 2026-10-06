using MongoDB.Driver;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOPlato
{
    private readonly IMongoCollection<PlatoDoc> _platos = Conexion.GetCollection<PlatoDoc>("platos");
    private readonly IMongoCollection<TipoPlatoDoc> _tipos = Conexion.GetCollection<TipoPlatoDoc>("tipos_plato");
    private readonly IMongoCollection<StockDoc> _stocks = Conexion.GetCollection<StockDoc>("stocks");

    public List<Plato> Listar() => Ordenar(_platos.Find(_ => true).ToList());

    public List<Plato> ListarDisponibles() => Ordenar(_platos.Find(p => p.Disponible).ToList());

    public List<Plato> ListarPorTipo(int idTipo) =>
        _platos.Find(p => p.IdTipo == idTipo && p.Disponible).ToList()
            .OrderBy(p => p.Nombre).Select(Mapear).ToList();

    public Plato? BuscarPorId(int id)
    {
        var d = _platos.Find(p => p.Id == id).FirstOrDefault();
        return d == null ? null : Mapear(d);
    }

    public void Insertar(Plato p)
    {
        var doc = new PlatoDoc
        {
            Id = Conexion.SiguienteId("platos"),
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Precio = p.Precio,
            Imagen = p.Imagen,
            IdTipo = p.IdTipo,
            Disponible = true
        };
        _platos.InsertOne(doc);
        _stocks.InsertOne(new StockDoc { Id = Conexion.SiguienteId("stocks"), IdPlato = doc.Id, Cantidad = 50, StockMinimo = 5 });
    }

    public void Actualizar(Plato p)
    {
        var update = Builders<PlatoDoc>.Update
            .Set(x => x.Nombre, p.Nombre)
            .Set(x => x.Descripcion, p.Descripcion)
            .Set(x => x.Precio, p.Precio)
            .Set(x => x.IdTipo, p.IdTipo);
        if (p.Imagen != null) update = update.Set(x => x.Imagen, p.Imagen);
        _platos.UpdateOne(x => x.Id == p.IdPlato, update);
    }

    public void CambiarDisponibilidad(int id, bool disponible)
    {
        _platos.UpdateOne(x => x.Id == id, Builders<PlatoDoc>.Update.Set(x => x.Disponible, disponible));
    }

    private List<Plato> Ordenar(List<PlatoDoc> docs)
    {
        var tipos = _tipos.Find(_ => true).ToList().ToDictionary(t => t.Id, t => t.Nombre);
        return docs
            .OrderBy(p => tipos.GetValueOrDefault(p.IdTipo, ""))
            .ThenBy(p => p.Nombre)
            .Select(Mapear)
            .ToList();
    }

    private Plato Mapear(PlatoDoc d) => new()
    {
        IdPlato = d.Id,
        Nombre = d.Nombre,
        Descripcion = d.Descripcion,
        Precio = d.Precio,
        Imagen = d.Imagen,
        IdTipo = d.IdTipo,
        TipoNombre = _tipos.Find(t => t.Id == d.IdTipo).FirstOrDefault()?.Nombre,
        Disponible = d.Disponible,
        Stock = _stocks.Find(s => s.IdPlato == d.Id).FirstOrDefault()?.Cantidad ?? 0
    };
}
