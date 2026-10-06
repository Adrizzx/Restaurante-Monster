using Microsoft.AspNetCore.Mvc;
using RestauranteMonster.Data;
using RestauranteMonster.Helpers;
using RestauranteMonster.Models;

namespace RestauranteMonster.Controllers;

[Route("Admin/[action]")]
public class AdminController : BaseController
{
    private readonly IWebHostEnvironment _env;
    public AdminController(IWebHostEnvironment env) => _env = env;

    // ─── DASHBOARD ────────────────────────────────────────────────────────────

    public IActionResult Dashboard()
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        try
        {
            ViewBag.Estadisticas = new DAOFactura().EstadisticasHoy();
            ViewBag.TotalPlatos  = new DAOPlato().Listar().Count;
            ViewBag.TotalBebidas = new DAOBebida().Listar().Count;
            ViewBag.StockBajo    = new DAOStock().ListarStockBajo();
        }
        catch (Exception ex) { ViewBag.Error = ex.Message; }
        return View();
    }

    // ─── PLATOS ───────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Platos(int? editar = null)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        ViewBag.Platos = new DAOPlato().Listar();
        ViewBag.Tipos  = new DAOTipoPlato().Listar();
        if (editar.HasValue) ViewBag.PlatoEditar = new DAOPlato().BuscarPorId(editar.Value);
        return View();
    }

    [HttpPost]
    [ActionName("Platos")]
    [RequestFormLimits(MultipartBodyLengthLimit = 4 * 1024 * 1024)]
    public async Task<IActionResult> GuardarPlato(int idPlato, string nombre, string? descripcion,
        decimal precio, int idTipo, IFormFile? imagen)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        try
        {
            var rutaImagen = await SubidaImagen.GuardarAsync(imagen, _env);
            var p = new Plato { IdPlato = idPlato, Nombre = nombre, Descripcion = descripcion,
                                Precio  = precio,  IdTipo  = idTipo, Imagen = rutaImagen };
            if (idPlato == 0) new DAOPlato().Insertar(p);
            else              new DAOPlato().Actualizar(p);
            TempData["Success"] = idPlato == 0 ? "Plato agregado exitosamente." : "Plato actualizado.";
        }
        catch (Exception ex) { TempData["Error"] = "Error: " + ex.Message; }
        return RedirectToAction("Platos");
    }

    [HttpGet]
    public IActionResult TogglePlato(int id, bool disponible)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        new DAOPlato().CambiarDisponibilidad(id, disponible);
        return RedirectToAction("Platos");
    }

    // ─── BEBIDAS ──────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Bebidas(int? editar = null)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        ViewBag.Bebidas = new DAOBebida().Listar();
        if (editar.HasValue) ViewBag.BebidaEditar = new DAOBebida().BuscarPorId(editar.Value);
        return View();
    }

    [HttpPost]
    [ActionName("Bebidas")]
    [RequestFormLimits(MultipartBodyLengthLimit = 4 * 1024 * 1024)]
    public async Task<IActionResult> GuardarBebida(int idBebida, string nombre, string? descripcion,
        decimal precio, IFormFile? imagen)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        try
        {
            var rutaImagen = await SubidaImagen.GuardarAsync(imagen, _env);
            var b = new Bebida { IdBebida = idBebida, Nombre = nombre, Descripcion = descripcion,
                                 Precio   = precio,   Imagen  = rutaImagen };
            if (idBebida == 0) new DAOBebida().Insertar(b);
            else               new DAOBebida().Actualizar(b);
            TempData["Success"] = idBebida == 0 ? "Bebida agregada exitosamente." : "Bebida actualizada.";
        }
        catch (Exception ex) { TempData["Error"] = "Error: " + ex.Message; }
        return RedirectToAction("Bebidas");
    }

    [HttpGet]
    public IActionResult ToggleBebida(int id, bool disponible)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        new DAOBebida().CambiarDisponibilidad(id, disponible);
        return RedirectToAction("Bebidas");
    }

    // ─── CATEGORIAS ───────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Categorias(int? editar = null)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        ViewBag.Tipos = new DAOTipoPlato().Listar();
        if (editar.HasValue) ViewBag.TipoEditar = new DAOTipoPlato().BuscarPorId(editar.Value);
        return View();
    }

    [HttpPost]
    [ActionName("Categorias")]
    public IActionResult GuardarCategoria(int idTipo, string nombre)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        try
        {
            if (idTipo == 0) new DAOTipoPlato().Insertar(nombre);
            else             new DAOTipoPlato().Actualizar(idTipo, nombre);
            TempData["Success"] = idTipo == 0 ? "Categoría agregada." : "Categoría actualizada.";
        }
        catch (Exception ex) { TempData["Error"] = "Error: " + ex.Message; }
        return RedirectToAction("Categorias");
    }

    [HttpGet]
    public IActionResult EliminarCategoria(int id)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        if (new DAOTipoPlato().TienePlatos(id))
        {
            TempData["Error"] = "No se puede eliminar: la categoría tiene platos asociados.";
            return RedirectToAction("Categorias");
        }
        new DAOTipoPlato().Eliminar(id);
        TempData["Success"] = "Categoría eliminada.";
        return RedirectToAction("Categorias");
    }

    // ─── CLIENTES ─────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Clientes(int? editar = null)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        ViewBag.Clientes = new DAOCliente().Listar();
        if (editar.HasValue) ViewBag.ClienteEditar = new DAOCliente().BuscarPorId(editar.Value);
        return View();
    }

    [HttpPost]
    [ActionName("Clientes")]
    public IActionResult GuardarCliente(int idCliente, int idUsuario, string nombre, string apellido,
        string correo, string? telefono, string? direccion, string? fechaNacimiento, string? contrasena)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        try
        {
            var dao = new DAOUsuario();
            if (idCliente == 0)
            {
                if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 6)
                { TempData["Error"] = "La contraseña debe tener al menos 6 caracteres."; return RedirectToAction("Clientes"); }
                if (dao.ExisteCorreo(correo))
                { TempData["Error"] = "El correo ya está registrado."; return RedirectToAction("Clientes"); }

                var u = new Usuario { Nombre = nombre, Apellido = apellido, Correo = correo,
                                      Contrasena = contrasena, Telefono = telefono, IdRol = 3 };
                int nuevoId = dao.Insertar(u);
                new DAOCliente().Insertar(new Cliente { IdUsuario = nuevoId, Direccion = direccion,
                                                        FechaNacimiento = fechaNacimiento });
            }
            else
            {
                var c = new Cliente { IdUsuario = idUsuario, Nombre = nombre, Apellido = apellido,
                                      Correo    = correo,    Telefono = telefono, Direccion = direccion,
                                      FechaNacimiento = fechaNacimiento };
                new DAOCliente().Actualizar(c);
            }
            TempData["Success"] = idCliente == 0 ? "Cliente registrado." : "Cliente actualizado.";
        }
        catch (Exception ex) { TempData["Error"] = "Error: " + ex.Message; }
        return RedirectToAction("Clientes");
    }

    // ─── EMPLEADOS ────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Empleados(int? editar = null)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        ViewBag.Empleados = new DAOEmpleado().Listar();
        if (editar.HasValue) ViewBag.EmpleadoEditar = new DAOEmpleado().BuscarPorId(editar.Value);
        return View();
    }

    [HttpPost]
    [ActionName("Empleados")]
    public IActionResult GuardarEmpleado(int idUsuario, string nombre, string apellido,
        string correo, string? telefono, string? contrasena)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        try
        {
            var e = new Empleado { IdUsuario = idUsuario, Nombre = nombre, Apellido = apellido,
                                   Correo    = correo,    Telefono = telefono };
            if (idUsuario == 0)
            {
                if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 6)
                { TempData["Error"] = "La contraseña debe tener al menos 6 caracteres."; return RedirectToAction("Empleados"); }
                if (new DAOUsuario().ExisteCorreo(correo))
                { TempData["Error"] = "El correo ya está registrado."; return RedirectToAction("Empleados"); }
                new DAOEmpleado().Insertar(e, contrasena!);
            }
            else
                new DAOEmpleado().Actualizar(e);

            TempData["Success"] = idUsuario == 0 ? "Empleado registrado." : "Empleado actualizado.";
        }
        catch (Exception ex) { TempData["Error"] = "Error: " + ex.Message; }
        return RedirectToAction("Empleados");
    }

    [HttpGet]
    public IActionResult ToggleEmpleado(int id, int estado)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        new DAOEmpleado().CambiarEstado(id, estado);
        return RedirectToAction("Empleados");
    }

    // ─── STOCK ────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Stock()
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        ViewBag.Stock = new DAOStock().ListarTodo();
        return View();
    }

    [HttpPost]
    [ActionName("Stock")]
    public IActionResult ActualizarStock(int idStock, int cantidad, int stockMinimo)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        try
        {
            new DAOStock().ActualizarCantidad(idStock, cantidad, stockMinimo);
            TempData["Success"] = "Stock actualizado correctamente.";
        }
        catch (Exception ex) { TempData["Error"] = "Error: " + ex.Message; }
        return RedirectToAction("Stock");
    }

    [HttpPost]
    public IActionResult ActualizarTodoStock(int[] ids, int[] cantidades, int[] minimos)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        try
        {
            var dao = new DAOStock();
            for (int i = 0; i < ids.Length; i++)
                dao.ActualizarCantidad(ids[i], cantidades[i], minimos[i]);
            TempData["Success"] = $"Stock actualizado: {ids.Length} ítems guardados.";
        }
        catch (Exception ex) { TempData["Error"] = "Error: " + ex.Message; }
        return RedirectToAction("Stock");
    }

    // ─── FACTURAS ─────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Facturas()
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        ViewBag.Facturas = new DAOFactura().Listar();
        return View();
    }

    [HttpGet]
    public IActionResult VerFactura(int id)
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        var factura = new DAOFactura().BuscarPorId(id);
        if (factura == null) return RedirectToAction("Facturas");
        factura.Detalles = new DAOPedido().ObtenerDetalles(factura.IdPedido);
        return View("~/Views/Admin/DetalleFactura.cshtml", factura);
    }

    // ─── REPORTES ─────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Reportes()
    {
        var r = CheckRole("ADMIN"); if (r != null) return r;
        ViewBag.Estadisticas = new DAOFactura().EstadisticasHoy();
        ViewBag.Facturas     = new DAOFactura().Listar();
        return View();
    }
}
