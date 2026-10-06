using Microsoft.AspNetCore.Mvc;
using RestauranteMonster.Data;
using RestauranteMonster.Models;

namespace RestauranteMonster.Controllers;

public class CarritoController : BaseController
{
    [HttpGet]
    [Route("Carrito")]
    public IActionResult Ver()
        => RedirectToAction("Carrito", "Cliente");

    // Endpoint para +/- en tarjetas sin recargar página
    [HttpPost]
    [Route("Carrito/Ajustar")]
    public IActionResult Ajustar(string tipo, int id, int cantidad)
    {
        var carrito = GetCarrito();
        var item = carrito.FirstOrDefault(d =>
            (tipo == "PLATO"  && d.IdPlato  == id) ||
            (tipo == "BEBIDA" && d.IdBebida == id));

        if (item == null && cantidad > 0)
        {
            var det = new DetallePedido { TipoItem = tipo, Cantidad = cantidad };
            if (tipo == "PLATO")
            {
                var p = new DAOPlato().BuscarPorId(id);
                if (p != null) { det.IdPlato = id; det.NombreItem = p.Nombre; det.PrecioUnitario = (double)p.Precio; det.Imagen = p.Imagen; }
            }
            else
            {
                var b = new DAOBebida().BuscarPorId(id);
                if (b != null) { det.IdBebida = id; det.NombreItem = b.Nombre; det.PrecioUnitario = (double)b.Precio; det.Imagen = b.Imagen; }
            }
            det.Subtotal = det.PrecioUnitario * cantidad;
            if (det.NombreItem != null) carrito.Add(det);
        }
        else if (item != null)
        {
            if (cantidad <= 0) carrito.Remove(item);
            else { item.Cantidad = cantidad; item.Subtotal = item.PrecioUnitario * cantidad; }
        }

        GuardarCarrito(carrito);
        return Ok();
    }

    [HttpPost]
    [Route("Carrito")]
    public async Task<IActionResult> Procesar(string accion, string? tipo, int id = 0,
        int cantidad = 1, int idx = 0, string? origen = null)
    {
        var carrito = GetCarrito();

        switch (accion)
        {
            case "agregar":
            {
                if (cantidad < 1) cantidad = 1;
                bool encontrado = false;
                foreach (var d in carrito)
                {
                    bool mismo = (tipo == "PLATO"  && d.IdPlato  == id) ||
                                 (tipo == "BEBIDA" && d.IdBebida == id);
                    if (mismo)
                    {
                        d.Cantidad   += cantidad;
                        d.Subtotal    = d.PrecioUnitario * d.Cantidad;
                        encontrado    = true;
                        break;
                    }
                }
                if (!encontrado)
                {
                    var det = new DetallePedido { TipoItem = tipo, Cantidad = cantidad };
                    if (tipo == "PLATO")
                    {
                        var p = new DAOPlato().BuscarPorId(id);
                        if (p != null)
                        {
                            det.IdPlato         = id;
                            det.NombreItem      = p.Nombre;
                            det.PrecioUnitario  = (double)p.Precio;
                            det.Imagen          = p.Imagen;
                        }
                    }
                    else
                    {
                        var b = new DAOBebida().BuscarPorId(id);
                        if (b != null)
                        {
                            det.IdBebida        = id;
                            det.NombreItem      = b.Nombre;
                            det.PrecioUnitario  = (double)b.Precio;
                            det.Imagen          = b.Imagen;
                        }
                    }
                    det.Subtotal = det.PrecioUnitario * det.Cantidad;
                    carrito.Add(det);
                }
                break;
            }
            case "modificar":
                if (idx >= 0 && idx < carrito.Count)
                {
                    if (cantidad <= 0)
                        carrito.RemoveAt(idx);
                    else
                    {
                        carrito[idx].Cantidad  = cantidad;
                        carrito[idx].Subtotal  = carrito[idx].PrecioUnitario * cantidad;
                    }
                }
                break;

            case "eliminar":
                if (idx >= 0 && idx < carrito.Count)
                    carrito.RemoveAt(idx);
                break;

            case "limpiar":
                carrito.Clear();
                break;
        }

        GuardarCarrito(carrito);

        if (origen == "mesero")
            return RedirectToAction("TomarPedido", "Mesero");
        return RedirectToAction("Carrito", "Cliente");
    }
}
