using Microsoft.AspNetCore.Mvc;
using RestauranteMonster.Data;
using RestauranteMonster.Helpers;
using RestauranteMonster.Models;

namespace RestauranteMonster.Controllers;

public class SeguridadController : BaseController
{
    [HttpGet]
    public IActionResult Login()
    {
        if (GetUsuarioActual() != null) return RedirigirPorRol();
        return View();
    }

    [HttpPost]
    public IActionResult Login(string correo, string contrasena)
    {
        try
        {
            var usuario = new DAOUsuario().Autenticar(correo, contrasena);
            if (usuario == null)
            {
                ViewBag.Error = "Credenciales incorrectas o cuenta desactivada.";
                return View();
            }
            HttpContext.Session.SetObject("usuario", usuario);
            return RedirigirPorRol(usuario);
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error de conexión: " + ex.Message;
            return View();
        }
    }

    [HttpGet]
    public IActionResult Registro()
    {
        if (GetUsuarioActual() != null) return RedirigirPorRol();
        return View();
    }

    [HttpPost]
    public IActionResult Registro(string nombre, string apellido, string correo,
        string contrasena, string? telefono)
    {
        try
        {
            if (contrasena.Length < 6)
            {
                ViewBag.Error = "La contraseña debe tener al menos 6 caracteres.";
                return View();
            }
            var dao = new DAOUsuario();
            if (dao.ExisteCorreo(correo))
            {
                ViewBag.Error = "Este correo ya está registrado.";
                return View();
            }
            var u = new Usuario
            {
                Nombre     = nombre,
                Apellido   = apellido,
                Correo     = correo,
                Contrasena = contrasena,
                Telefono   = telefono,
                IdRol      = 3
            };
            int idUsuario = dao.Insertar(u);
            new DAOCliente().Insertar(new Cliente { IdUsuario = idUsuario });

            TempData["Success"] = "Cuenta creada exitosamente. Por favor inicia sesión.";
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error al registrarse: " + ex.Message;
            return View();
        }
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    private IActionResult RedirigirPorRol(Usuario? u = null)
    {
        u ??= GetUsuarioActual();
        return u?.RolNombre switch
        {
            "ADMIN"   => RedirectToAction("Dashboard", "Admin"),
            "MESERO"  => RedirectToAction("Dashboard", "Mesero"),
            "CLIENTE" => RedirectToAction("Menu", "Cliente"),
            _         => RedirectToAction("Login")
        };
    }
}
