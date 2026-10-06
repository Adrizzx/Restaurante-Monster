using Microsoft.AspNetCore.Mvc;
using RestauranteMonster.Helpers;
using RestauranteMonster.Models;

namespace RestauranteMonster.Controllers;

public class BaseController : Controller
{
    protected Usuario? GetUsuarioActual()
        => HttpContext.Session.GetObject<Usuario>("usuario");

    protected IActionResult? CheckRole(params string[] roles)
    {
        var usuario = GetUsuarioActual();
        if (usuario == null)
            return RedirectToAction("Login", "Seguridad");
        if (!roles.Contains(usuario.RolNombre))
            return View("~/Views/Shared/NoAutorizado.cshtml");
        return null;
    }

    protected List<DetallePedido> GetCarrito()
        => HttpContext.Session.GetObject<List<DetallePedido>>("carrito") ?? new();

    protected void GuardarCarrito(List<DetallePedido> carrito)
        => HttpContext.Session.SetObject("carrito", carrito);
}
