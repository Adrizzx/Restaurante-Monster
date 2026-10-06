namespace RestauranteMonster.Models;

public class Stock
{
    public int IdStock { get; set; }
    public int IdPlato { get; set; }
    public int IdBebida { get; set; }
    public int Cantidad { get; set; }
    public int StockMinimo { get; set; }
    public string NombreItem { get; set; } = string.Empty;
    public string TipoItem { get; set; } = string.Empty;
    public double Precio { get; set; }
    public bool StockBajo => Cantidad <= StockMinimo;
}
