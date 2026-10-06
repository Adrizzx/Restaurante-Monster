using Microsoft.AspNetCore.Mvc;
using RestauranteMonster.Data;
using RestauranteMonster.Helpers;
using RestauranteMonster.Models;

namespace RestauranteMonster.Controllers;

[Route("Cliente/[action]")]
public class ClienteController : BaseController
{
    // ─── MENÚ ─────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Menu()
    {
        var r = CheckRole("CLIENTE"); if (r != null) return r;
        CargarMenu();
        return View();
    }

    // ─── CARRITO ──────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Carrito()
    {
        var r = CheckRole("CLIENTE"); if (r != null) return r;
        var carrito = GetCarrito();
        double subtotal = carrito.Sum(d => d.Subtotal);
        ViewBag.Subtotal = subtotal;
        ViewBag.Iva      = subtotal * 0.12;
        ViewBag.Total    = subtotal + subtotal * 0.12;
        return View(carrito);
    }

    // ─── PROCESAR PEDIDO ──────────────────────────────────────────────────────

    [HttpPost]
    public IActionResult Pedido()
    {
        var r = CheckRole("CLIENTE"); if (r != null) return r;
        var carrito = GetCarrito();

        if (!carrito.Any())
        {
            ViewBag.Error = "El carrito está vacío. Agrega al menos un ítem.";
            CargarMenu();
            return View("Menu");
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
                    CargarMenu();
                    return View("Menu");
                }
            }

            var usuario  = GetUsuarioActual()!;
            var daoPedido = new DAOPedido();
            int idPedido  = daoPedido.Crear(usuario.IdUsuario, 0, "KIOSCO");

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
            HttpContext.Session.SetInt32("ultimaFactura", idFactura);

            return RedirectToAction("Factura", new { id = idFactura });
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al procesar pedido: " + ex.Message;
            CargarMenu();
            return View("Menu");
        }
    }

    // ─── FACTURA ──────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Factura(int? id)
    {
        var r = CheckRole("CLIENTE"); if (r != null) return r;
        if (!id.HasValue) id = HttpContext.Session.GetInt32("ultimaFactura");
        if (!id.HasValue) return RedirectToAction("Menu");

        var factura = new DAOFactura().BuscarPorId(id.Value);
        if (factura == null) return RedirectToAction("Menu");
        factura.Detalles = new DAOPedido().ObtenerDetalles(factura.IdPedido);
        return View(factura);
    }

    // ─── HELPERS ──────────────────────────────────────────────────────────────

    private void CargarMenu()
    {
        ViewBag.Platos  = new DAOPlato().ListarDisponibles();
        ViewBag.Bebidas = new DAOBebida().ListarDisponibles();
        ViewBag.Tipos   = new DAOTipoPlato().Listar();
        ViewBag.Carrito = GetCarrito();
    }
}
