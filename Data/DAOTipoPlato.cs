using MongoDB.Driver;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOTipoPlato
{
    private readonly IMongoCollection<TipoPlatoDoc> _tipos = Conexion.GetCollection<TipoPlatoDoc>("tipos_plato");
    private readonly IMongoCollection<PlatoDoc> _platos = Conexion.GetCollection<PlatoDoc>("platos");

    public List<TipoPlato> Listar()
    {
        return _tipos.Find(_ => true).ToList()
            .OrderBy(t => t.Nombre)
            .Select(t => new TipoPlato { IdTipo = t.Id, Nombre = t.Nombre })
            .ToList();
    }

    public TipoPlato? BuscarPorId(int id)
    {
        var d = _tipos.Find(t => t.Id == id).FirstOrDefault();
        return d == null ? null : new TipoPlato { IdTipo = d.Id, Nombre = d.Nombre };
    }

    public void Insertar(string nombre)
    {
        _tipos.InsertOne(new TipoPlatoDoc { Id = Conexion.SiguienteId("tipos_plato"), Nombre = nombre });
    }

    public void Actualizar(int id, string nombre)
    {
        _tipos.UpdateOne(t => t.Id == id, Builders<TipoPlatoDoc>.Update.Set(t => t.Nombre, nombre));
    }

    public bool TienePlatos(int idTipo)
    {
        return _platos.Find(p => p.IdTipo == idTipo).Any();
    }

    public void Eliminar(int id)
    {
        _tipos.DeleteOne(t => t.Id == id);
    }
}
