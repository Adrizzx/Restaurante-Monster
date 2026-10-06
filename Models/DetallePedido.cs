namespace RestauranteMonster.Models;

public class DetallePedido
{
    public int IdDetalle { get; set; }
    public int IdPedido { get; set; }
    public int IdPlato { get; set; }
    public int IdBebida { get; set; }
    public int Cantidad { get; set; }
    public double PrecioUnitario { get; set; }
    public double Subtotal { get; set; }
    public string NombreItem { get; set; } = string.Empty;
    public string? Imagen { get; set; }
    public string? TipoItem { get; set; }
}
