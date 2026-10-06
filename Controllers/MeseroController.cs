using Microsoft.AspNetCore.Mvc;
using RestauranteMonster.Data;
using RestauranteMonster.Helpers;
using RestauranteMonster.Models;

namespace RestauranteMonster.Controllers;

[Route("Mesero/[action]")]
public class MeseroController : BaseController
{
    // ─── DASHBOARD ────────────────────────────────────────────────────────────

    public IActionResult Dashboard()
    {
        var r = CheckRole("MESERO", "ADMIN"); if (r != null) return r;
        ViewBag.StockBajo = new DAOStock().ListarStockBajo();
        return View();
    }

    // ─── TOMAR PEDIDO ─────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult TomarPedido()
    {
        var r = CheckRole("MESERO", "ADMIN"); if (r != null) return r;
        CargarMenuMesero();
        return View();
    }

    [HttpPost]
    [ActionName("TomarPedido")]
    public IActionResult ProcesarPedido()
    {
        var r = CheckRole("MESERO", "ADMIN"); if (r != null) return r;
        var carrito = GetCarrito();

        if (!carrito.Any())
        {
            ViewBag.Error = "El carrito está vacío. Agrega al menos un ítem.";
            CargarMenuMesero();
            return View("TomarPedido");
        }

        try
        {
            var daoStock = new DAOStock();
            foreach (var d in carrito)
            {
                int stock = daoStock.GetStockDisponible(d.IdPlato, d.IdBebida);
                if (stock < d.Cantidad)
                {
                    ViewBag.Error = $"Stock insuficiente para: {d.NombreItem} (disponible: {stock})";
                    CargarMenuMesero();
                    return View("TomarPedido");
                }
            }

            var usuario       = GetUsuarioActual()!;
            var clienteActual = HttpContext.Session.GetObject<Cliente>("clienteActual");
            int idUsuario     = clienteActual?.IdUsuario ?? 0;
            int idMesero      = usuario.IdUsuario;

            var daoPedido = new DAOPedido();
            int idPedido  = daoPedido.Crear(idUsuario, idMesero, "MESA");

            foreach (var d in carrito)
            {
                d.IdPedido = idPedido;
                daoPedido.AgregarDetalle(d);
                daoStock.DescontarStock(d.IdPlato, d.IdBebida, d.Cantidad);
            }

            int idFactura = new DAOFactura().GenerarFactura(idPedido, carrito);
            daoPedido.ActualizarEstado(idPedido, "PAGADO");

            carrito.Clear();
            GuardarCarrito(carrito);
            HttpContext.Session.Remove("clienteActual");
            HttpContext.Session.SetInt32("ultimaFactura", idFactura);

            return RedirectToAction("Factura", new { id = idFactura });
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al procesar pedido: " + ex.Message;
            CargarMenuMesero();
            return View("TomarPedido");
        }
    }

    // ─── REGISTRAR CLIENTE ────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult RegistrarCliente()
    {
        var r = CheckRole("MESERO", "ADMIN"); if (r != null) return r;
        return View();
    }

    [HttpPost]
    [ActionName("RegistrarCliente")]
    public IActionResult GuardarCliente(string nombre, string apellido, string correo,
        string? telefono, string? direccion)
    {
        var r = CheckRole("MESERO", "ADMIN"); if (r != null) return r;
        try
        {
            var dao = new DAOUsuario();
            int idUsuario;
            if (dao.ExisteCorreo(correo))
            {
                var usuarioExistente = new DAOCliente().Listar()
                    .FirstOrDefault(c => c.Correo == correo);
                if (usuarioExistente == null)
                { ViewBag.Error = "Correo registrado pero sin perfil de cliente."; return View("RegistrarCliente"); }
                idUsuario = usuarioExistente.IdUsuario;
            }
            else
            {
                var tempPass = "Cliente#" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var u = new Usuario { Nombre = nombre, Apellido = apellido, Correo = correo,
                                      Contrasena = tempPass, Telefono = telefono, IdRol = 3 };
                idUsuario = dao.Insertar(u);
                new DAOCliente().Insertar(new Cliente { IdUsuario = idUsuario, Direccion = direccion });
            }

            var cliente = new DAOCliente().BuscarPorUsuario(idUsuario)
                ?? new Cliente { IdUsuario = idUsuario, Nombre = nombre, Apellido = apellido, Correo = correo };
            HttpContext.Session.SetObject("clienteActual", cliente);
            return RedirectToAction("TomarPedido");
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error: " + ex.Message;
            return View("RegistrarCliente");
        }
    }

    // ─── FACTURA ──────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Factura(int? id)
    {
        var r = CheckRole("MESERO", "ADMIN"); if (r != null) return r;
        if (!id.HasValue) id = HttpContext.Session.GetInt32("ultimaFactura");
        if (!id.HasValue) return RedirectToAction("Dashboard");

        var factura = new DAOFactura().BuscarPorId(id.Value);
        if (factura == null) return RedirectToAction("Dashboard");
        factura.Detalles = new DAOPedido().ObtenerDetalles(factura.IdPedido);
        return View(factura);
    }

    // ─── ALERTA STOCK ─────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult AlertaStock()
    {
        var r = CheckRole("MESERO", "ADMIN"); if (r != null) return r;
        ViewBag.StockBajo = new DAOStock().ListarStockBajo();
        return View();
    }

    // ─── HELPERS ──────────────────────────────────────────────────────────────

    private void CargarMenuMesero()
    {
        ViewBag.Platos       = new DAOPlato().ListarDisponibles();
        ViewBag.Bebidas      = new DAOBebida().ListarDisponibles();
        ViewBag.Tipos        = new DAOTipoPlato().Listar();
        ViewBag.StockBajo    = new DAOStock().ListarStockBajo();
        ViewBag.Carrito      = GetCarrito();
        ViewBag.ClienteActual = HttpContext.Session.GetObject<Cliente>("clienteActual");
    }
}
