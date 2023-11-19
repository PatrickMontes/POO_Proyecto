using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using POO2_Proyecto.Models;

[Authorize(Roles = "Cliente")]
public class ECommerceController : Controller
{
    private readonly IConfiguration _configuration;

    public ECommerceController(IConfiguration configuration)
    {
        _configuration = configuration;
    }



    private IEnumerable<Libro> ObtenerLibros()
    {
        List<Libro> lista_libros = new List<Libro>();

        using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            con.Open();

            using (SqlCommand cmd = new SqlCommand("sp_listar_libros", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        lista_libros.Add(new Libro
                        {
                            IdLibro = (int)rd["IdLibro"],
                            Título = rd["Título"].ToString(),
                            Imagen = rd["Imagen"].ToString(),
                            Precio = (decimal)rd["Precio"],
                            Stock = (int)rd["Stock"]
                        });
                    }
                }
            }
        }

        return lista_libros;
    }



    private Libro BuscarLibro(int id)
    {
        return ObtenerLibros().FirstOrDefault(c => c.IdLibro == id);
    }



    [HttpGet]
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("carrito") == null)
        {
            HttpContext.Session.SetString("carrito", JsonConvert.SerializeObject(new List<Libro>()));
        }
        return View(ObtenerLibros());
    }

    [HttpGet]
    public IActionResult BuscarLibro(string nombreLibro)
    {
        IEnumerable<Libro> libros = ObtenerLibros();

        if (!string.IsNullOrEmpty(nombreLibro))
        {
            libros = libros.Where(l => l.Título.Contains(nombreLibro, StringComparison.OrdinalIgnoreCase));
        }

        return View("Index", libros);
    }


    [HttpGet]
    public IActionResult Select(int? id = null)
    {
        if (id == null) return RedirectToAction("Index");

        return View(BuscarLibro(id.Value));
    }



    [HttpPost]
    public IActionResult Select(int IdLibro, int cantidad)
    {
        string mensaje = "";
        Libro reg = BuscarLibro(IdLibro);

        List<Registro> auxiliar = JsonConvert.DeserializeObject<List<Registro>>(HttpContext.Session.GetString("carrito"));

        Registro item = auxiliar.FirstOrDefault(p => p.IdRegistro == IdLibro);
        if (item != null)
        {
            item.Cantidad += cantidad;
        }
        else
        {
            item = new Registro()
            {
                IdRegistro = reg.IdLibro,
                Título = reg.Título,
                Imagen = reg.Imagen,
                Precio = reg.Precio,
                Cantidad = cantidad
            };
            auxiliar.Add(item);
        }

        // Actualiza la sesión con el mismo nombre "carrito"
        HttpContext.Session.SetString("carrito", JsonConvert.SerializeObject(auxiliar));

        mensaje = $"Tiene un pedido de {item.Cantidad} unidades del libro {item.Título}";

        return RedirectToAction("ShowAlert", new { msj = mensaje });
    }



    [HttpGet]
    public IActionResult Carrito()
    {
        if (HttpContext.Session.GetString("carrito") == null)
            return RedirectToAction("Index");

        List<Registro> auxiliar = JsonConvert.DeserializeObject<List<Registro>>(HttpContext.Session.GetString("carrito"));

        if (auxiliar.Count == 0)
            return RedirectToAction("Index");

        return View(auxiliar);
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        List<Registro> auxiliar = JsonConvert.DeserializeObject<List<Registro>>(HttpContext.Session.GetString("carrito"));

        Registro item = auxiliar.FirstOrDefault(s => s.IdRegistro == id);
        auxiliar.Remove(item);

        HttpContext.Session.SetString("carrito", JsonConvert.SerializeObject(auxiliar));

        return RedirectToAction("Carrito");
    }

    [HttpGet]
    public IActionResult Pedido()
    {
        List<Registro> auxiliar = JsonConvert.DeserializeObject<List<Registro>>(HttpContext.Session.GetString("carrito"));

        if (auxiliar.Count == 0) return RedirectToAction("Index");

        ViewBag.carrito = auxiliar;
        return View(new Pedido());
    }

    [HttpPost]
    public IActionResult Pedido(Pedido reg)
    {
        string mensaje = "";
        using (SqlConnection cn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            cn.Open();
            SqlTransaction tr = cn.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                IEnumerable<Registro> auxiliar = JsonConvert.DeserializeObject<List<Registro>>(HttpContext.Session.GetString("carrito"));

                SqlCommand cmd = new SqlCommand("sp_pedido_agregar", cn, tr);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@idPedido", SqlDbType.VarChar, 8).Direction = ParameterDirection.Output;
                cmd.Parameters.AddWithValue("@dniCliente", reg.dniCliente);
                cmd.Parameters.AddWithValue("@nomCliente", reg.nomCliente);
                cmd.Parameters.AddWithValue("@emailCliente", reg.emailCliente);
                cmd.Parameters.AddWithValue("@telefono", reg.telefono);

                cmd.ExecuteNonQuery();

                string idPedido = cmd.Parameters["@idPedido"].Value.ToString();

                foreach (var item in auxiliar)
                {
                    cmd = new SqlCommand(
                        "exec sp_pedidoDetalle_agregar @idPedido, @idLibro, @Precio, @Cantidad", cn, tr);
                    cmd.Parameters.AddWithValue("@idPedido", idPedido);
                    cmd.Parameters.AddWithValue("@idLibro", item.IdRegistro);
                    cmd.Parameters.AddWithValue("@precio", item.Precio);
                    cmd.Parameters.AddWithValue("@cantidad", item.Cantidad);
                    cmd.ExecuteNonQuery();
                }

                tr.Commit();
                mensaje = $"Se ha registrado el pedido de numero {idPedido} satisfactoriamente";
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                tr.Rollback();
            }
            finally
            {
                cn.Close();
            }
        }

        return RedirectToAction("ShowAlert", new { msj = mensaje });
    }

    [HttpGet]
    public IActionResult ShowAlert(string msj)
    {
        ViewBag.mensaje = msj;

        return View("ShowAlert");
    }

}
