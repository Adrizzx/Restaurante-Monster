namespace RestauranteMonster.Models;

public class Pedido
{
    public int IdPedido { get; set; }
    public int IdUsuario { get; set; }
    public int IdMesero { get; set; }
    public string? Fecha { get; set; }
    public string Estado { get; set; } = "PENDIENTE";
    public string TipoPedido { get; set; } = "MESA";
    public string? ClienteNombre { get; set; }
    public string? MeseroNombre { get; set; }
    public List<DetallePedido> Detalles { get; set; } = new();
}
