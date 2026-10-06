namespace RestauranteMonster.Models;

public class Plato
{
    public int IdPlato { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public string? Imagen { get; set; }
    public int IdTipo { get; set; }
    public string? TipoNombre { get; set; }
    public bool Disponible { get; set; } = true;
    public int Stock { get; set; }
}
