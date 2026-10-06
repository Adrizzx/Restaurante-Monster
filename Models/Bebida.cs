namespace RestauranteMonster.Models;

public class Bebida
{
    public int IdBebida { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public string? Imagen { get; set; }
    public bool Disponible { get; set; } = true;
    public int Stock { get; set; }
}
