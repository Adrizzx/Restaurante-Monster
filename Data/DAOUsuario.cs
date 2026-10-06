using MongoDB.Driver;
using RestauranteMonster.Helpers;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOUsuario
{
    private readonly IMongoCollection<UsuarioDoc> _usuarios = Conexion.GetCollection<UsuarioDoc>("usuarios");
    private readonly IMongoCollection<RolDoc> _roles = Conexion.GetCollection<RolDoc>("roles");

    public Usuario? Autenticar(string correo, string contrasena)
    {
        var hash = Encriptador.Sha256(contrasena);
        var doc = _usuarios.Find(u => u.Correo == correo && u.Contrasena == hash && u.Estado == 1).FirstOrDefault();
        return doc == null ? null : Mapear(doc);
    }

    public List<Usuario> Listar()
    {
        var lista = _usuarios.Find(_ => true).ToList().Select(Mapear).ToList();
        return lista.OrderBy(u => u.Nombre).ToList();
    }

    public Usuario? BuscarPorId(int id)
    {
        var doc = _usuarios.Find(u => u.Id == id).FirstOrDefault();
        return doc == null ? null : Mapear(doc);
    }

    public bool ExisteCorreo(string correo)
    {
        return _usuarios.Find(u => u.Correo == correo).Any();
    }

    public int Insertar(Usuario u)
    {
        var doc = new UsuarioDoc
        {
            Id = Conexion.SiguienteId("usuarios"),
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Correo = u.Correo,
            Contrasena = Encriptador.Sha256(u.Contrasena),
            Telefono = u.Telefono,
            IdRol = u.IdRol,
            Estado = 1
        };
        _usuarios.InsertOne(doc);
        return doc.Id;
    }

    public void Actualizar(Usuario u)
    {
        var update = Builders<UsuarioDoc>.Update
            .Set(x => x.Nombre, u.Nombre)
            .Set(x => x.Apellido, u.Apellido)
            .Set(x => x.Correo, u.Correo)
            .Set(x => x.Telefono, u.Telefono)
            .Set(x => x.IdRol, u.IdRol);
        _usuarios.UpdateOne(x => x.Id == u.IdUsuario, update);
    }

    public void CambiarEstado(int id, int estado)
    {
        _usuarios.UpdateOne(x => x.Id == id, Builders<UsuarioDoc>.Update.Set(x => x.Estado, estado));
    }

    public void CambiarContrasena(int id, string nuevaContrasena)
    {
        _usuarios.UpdateOne(x => x.Id == id,
            Builders<UsuarioDoc>.Update.Set(x => x.Contrasena, Encriptador.Sha256(nuevaContrasena)));
    }

    private Usuario Mapear(UsuarioDoc d) => new()
    {
        IdUsuario = d.Id,
        Nombre = d.Nombre,
        Apellido = d.Apellido,
        Correo = d.Correo,
        Telefono = d.Telefono,
        IdRol = d.IdRol,
        RolNombre = _roles.Find(r => r.Id == d.IdRol).FirstOrDefault()?.Nombre ?? "",
        Estado = d.Estado
    };
}
