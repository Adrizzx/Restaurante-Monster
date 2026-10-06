namespace RestauranteMonster.Models;

public class Usuario
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public int IdRol { get; set; }
    public string RolNombre { get; set; } = string.Empty;
    public int Estado { get; set; } = 1;

    public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
}
