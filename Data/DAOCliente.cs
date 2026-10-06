using MongoDB.Driver;
using RestauranteMonster.Models;

namespace RestauranteMonster.Data;

public class DAOCliente
{
    private readonly IMongoCollection<ClienteDoc> _clientes = Conexion.GetCollection<ClienteDoc>("clientes");
    private readonly IMongoCollection<UsuarioDoc> _usuarios = Conexion.GetCollection<UsuarioDoc>("usuarios");
    private readonly IMongoCollection<PedidoDoc> _pedidos = Conexion.GetCollection<PedidoDoc>("pedidos");

    public List<Cliente> Listar() =>
        _clientes.Find(_ => true).ToList()
            .Select(Mapear).Where(c => c != null).Select(c => c!)
            .OrderBy(c => c.Nombre).ToList();

    public Cliente? BuscarPorId(int id)
    {
        var d = _clientes.Find(c => c.Id == id).FirstOrDefault();
        return d == null ? null : Mapear(d);
    }

    public Cliente? BuscarPorUsuario(int idUsuario)
    {
        var d = _clientes.Find(c => c.IdUsuario == idUsuario).FirstOrDefault();
        return d == null ? null : Mapear(d);
    }

    public void Insertar(Cliente c)
    {
        _clientes.InsertOne(new ClienteDoc
        {
            Id = Conexion.SiguienteId("clientes"),
            IdUsuario = c.IdUsuario,
            Direccion = c.Direccion,
            FechaNacimiento = string.IsNullOrEmpty(c.FechaNacimiento) ? null : c.FechaNacimiento
        });
    }

    public void Actualizar(Cliente c)
    {
        var updateUsuario = Builders<UsuarioDoc>.Update
            .Set(u => u.Nombre, c.Nombre)
            .Set(u => u.Apellido, c.Apellido)
            .Set(u => u.Correo, c.Correo)
            .Set(u => u.Telefono, c.Telefono);
        _usuarios.UpdateOne(u => u.Id == c.IdUsuario, updateUsuario);

        var updateCliente = Builders<ClienteDoc>.Update
            .Set(x => x.Direccion, c.Direccion)
            .Set(x => x.FechaNacimiento, string.IsNullOrEmpty(c.FechaNacimiento) ? null : c.FechaNacimiento);
        _clientes.UpdateOne(x => x.IdUsuario == c.IdUsuario, updateCliente);
    }

    public bool TienePedidosActivos(int idUsuario) =>
        _pedidos.Find(p => p.IdUsuario == idUsuario && p.Estado == "PENDIENTE").Any();

    private Cliente? Mapear(ClienteDoc d)
    {
        var u = _usuarios.Find(x => x.Id == d.IdUsuario).FirstOrDefault();
        if (u == null) return null;
        return new Cliente
        {
            IdCliente = d.Id,
            IdUsuario = d.IdUsuario,
            Nombre = u.Nombre,
            Apellido = u.Apellido,
            Correo = u.Correo,
            Telefono = u.Telefono,
            Direccion = d.Direccion,
            FechaNacimiento = d.FechaNacimiento,
            Estado = u.Estado
        };
    }
}
