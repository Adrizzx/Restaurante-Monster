namespace RestauranteMonster.Models;

public class Empleado
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public int Estado { get; set; } = 1;

    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
}
