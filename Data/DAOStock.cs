using MongoDB.Driver;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOStock
{
    private readonly IMongoCollection<StockDoc> _stocks = Conexion.GetCollection<StockDoc>("stocks");
    private readonly IMongoCollection<PlatoDoc> _platos = Conexion.GetCollection<PlatoDoc>("platos");
    private readonly IMongoCollection<BebidaDoc> _bebidas = Conexion.GetCollection<BebidaDoc>("bebidas");

    public List<Stock> ListarTodo()
    {
        var lista = new List<Stock>();
        var platosMap = _platos.Find(_ => true).ToList().ToDictionary(p => p.Id);
        var stocksPlato = _stocks.Find(s => s.IdPlato != null).ToList()
            .Where(s => platosMap.ContainsKey(s.IdPlato!.Value))
            .OrderBy(s => platosMap[s.IdPlato!.Value].Nombre);
        foreach (var s in stocksPlato)
        {
            var plato = platosMap[s.IdPlato!.Value];
            lista.Add(Mapear(s, plato.Nombre, plato.Precio, "PLATO"));
        }
        var bebidasMap = _bebidas.Find(_ => true).ToList().ToDictionary(b => b.Id);
        var stocksBebida = _stocks.Find(s => s.IdBebida != null).ToList()
            .Where(s => bebidasMap.ContainsKey(s.IdBebida!.Value))
            .OrderBy(s => bebidasMap[s.IdBebida!.Value].Nombre);
        foreach (var s in stocksBebida)
        {
            var bebida = bebidasMap[s.IdBebida!.Value];
            lista.Add(Mapear(s, bebida.Nombre, bebida.Precio, "BEBIDA"));
        }
        return lista;
    }

    public List<Stock> ListarStockBajo() =>
        ListarTodo().Where(s => s.StockBajo).OrderBy(s => s.NombreItem).ToList();

    public void ActualizarCantidad(int idStock, int cantidad, int stockMinimo)
    {
        var update = Builders<StockDoc>.Update.Set(s => s.Cantidad, cantidad).Set(s => s.StockMinimo, stockMinimo);
        _stocks.UpdateOne(s => s.Id == idStock, update);
    }

    public void DescontarStock(int idPlato, int idBebida, int cantidad)
    {
        StockDoc? stock = idPlato > 0
            ? _stocks.Find(s => s.IdPlato == idPlato).FirstOrDefault()
            : _stocks.Find(s => s.IdBebida == idBebida).FirstOrDefault();
        if (stock == null) return;
        var nuevaCantidad = stock.Cantidad > cantidad ? stock.Cantidad - cantidad : 0;
        _stocks.UpdateOne(s => s.Id == stock.Id, Builders<StockDoc>.Update.Set(s => s.Cantidad, nuevaCantidad));
    }

    public int GetStockDisponible(int idPlato, int idBebida)
    {
        var stock = idPlato > 0
            ? _stocks.Find(s => s.IdPlato == idPlato).FirstOrDefault()
            : _stocks.Find(s => s.IdBebida == idBebida).FirstOrDefault();
        return stock?.Cantidad ?? 0;
    }

    private static Stock Mapear(StockDoc d, string nombreItem, decimal precio, string tipoItem) => new()
    {
        IdStock = d.Id,
        IdPlato = d.IdPlato ?? 0,
        IdBebida = d.IdBebida ?? 0,
        Cantidad = d.Cantidad,
        StockMinimo = d.StockMinimo,
        NombreItem = nombreItem,
        TipoItem = tipoItem,
        Precio = (double)precio
    };
}
