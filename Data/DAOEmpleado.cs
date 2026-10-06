using MongoDB.Driver;
using RestauranteMonster.Helpers;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOEmpleado
{
    private readonly IMongoCollection<UsuarioDoc> _usuarios = Conexion.GetCollection<UsuarioDoc>("usuarios");
    private const int IdRolMesero = 2;

    public List<Empleado> Listar() =>
        _usuarios.Find(u => u.IdRol == IdRolMesero).ToList().OrderBy(u => u.Nombre).Select(Mapear).ToList();

    public Empleado? BuscarPorId(int id)
    {
        var d = _usuarios.Find(u => u.Id == id && u.IdRol == IdRolMesero).FirstOrDefault();
        return d == null ? null : Mapear(d);
    }

    public void Insertar(Empleado e, string contrasena)
    {
        _usuarios.InsertOne(new UsuarioDoc
        {
            Id = Conexion.SiguienteId("usuarios"),
            Nombre = e.Nombre,
            Apellido = e.Apellido,
            Correo = e.Correo,
            Contrasena = Encriptador.Sha256(contrasena),
            Telefono = e.Telefono,
            IdRol = IdRolMesero,
            Estado = 1
        });
    }

    public void Actualizar(Empleado e)
    {
        var update = Builders<UsuarioDoc>.Update
            .Set(x => x.Nombre, e.Nombre)
            .Set(x => x.Apellido, e.Apellido)
            .Set(x => x.Correo, e.Correo)
            .Set(x => x.Telefono, e.Telefono);
        _usuarios.UpdateOne(x => x.Id == e.IdUsuario && x.IdRol == IdRolMesero, update);
    }

    public void CambiarEstado(int id, int estado)
    {
        _usuarios.UpdateOne(x => x.Id == id, Builders<UsuarioDoc>.Update.Set(x => x.Estado, estado));
    }

    private static Empleado Mapear(UsuarioDoc d) => new()
    {
        IdUsuario = d.Id,
        Nombre = d.Nombre,
        Apellido = d.Apellido,
        Correo = d.Correo,
        Telefono = d.Telefono,
        Estado = d.Estado
    };
}
