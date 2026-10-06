namespace RestauranteMonster.Models;

public class Factura
{
    public int IdFactura { get; set; }
    public int IdPedido { get; set; }
    public string? Fecha { get; set; }
    public double Subtotal { get; set; }
    public double CargoServicio { get; set; }
    public double Iva { get; set; }
    public double Total { get; set; }
    public string Estado { get; set; } = "EMITIDA";
    public string? ClienteNombre { get; set; }
    public string? MeseroNombre { get; set; }
    public List<DetallePedido> Detalles { get; set; } = new();
}
