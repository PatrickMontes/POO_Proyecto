using Microsoft.AspNetCore.Mvc;
using POO2_Proyecto.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace POO2_Proyecto.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class LibroController : Controller
    {
        private readonly string connectionString;

        public LibroController(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IActionResult MantenimientoLibro()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                List<Libro> lista_libros = new();
                using (SqlCommand cmd = new("sp_listar_libros", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();

                    var rd = cmd.ExecuteReader();
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
                ViewBag.listado = lista_libros;
                return View();
            }
        }


        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Libro libro)
        {
            try
            {
                byte[] bytes;
                if (libro.File != null && libro.Título != null && libro.Precio != 0 && libro.Stock != 0)
                {
                    using (Stream fs = libro.File.OpenReadStream())
                    {
                        using (BinaryReader br = new(fs))
                        {
                            bytes = br.ReadBytes((int)fs.Length);
                            libro.Imagen = Convert.ToBase64String(bytes, 0, bytes.Length);

                            using (SqlConnection con = new SqlConnection(connectionString))
                            {
                                using (SqlCommand cmd = new("sp_insertar_libros", con))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add("@Título", SqlDbType.VarChar).Value = libro.Título;
                                    cmd.Parameters.Add("@Imagen", SqlDbType.VarChar).Value = libro.Imagen;
                                    cmd.Parameters.Add("@Precio", SqlDbType.Decimal).Value = libro.Precio;
                                    cmd.Parameters.Add("@Stock", SqlDbType.Int).Value = libro.Stock;
                                    con.Open();
                                    cmd.ExecuteNonQuery();
                                    con.Close();
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                ViewBag.error = e.Message;
                return View();
            }
            return RedirectToAction("MantenimientoLibro");
        }


        public IActionResult Editar(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                Libro registro = new();
                using (SqlCommand cmd = new("sp_buscar_libro", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IdLibro", SqlDbType.Int).Value = id;
                    con.Open();

                    SqlDataAdapter da = new(cmd);
                    DataTable dt = new();
                    da.Fill(dt);
                    registro.IdLibro = (int)dt.Rows[0][0];
                    registro.Título = dt.Rows[0][1].ToString();
                    registro.Imagen = dt.Rows[0][2].ToString();
                    registro.Precio = (decimal)dt.Rows[0][3];
                    registro.Stock = (int)dt.Rows[0][4];
                }
                return View(registro);
            }
        }

        [HttpPost]
        public IActionResult Editar(Libro libro)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string i;
                    if (libro.File == null)
                    {
                        i = "null";
                    }
                    else
                    {
                        byte[] bytes;
                        using (Stream fs = libro.File.OpenReadStream())
                        {
                            using (BinaryReader br = new(fs))
                            {
                                bytes = br.ReadBytes((int)fs.Length);
                                i = Convert.ToBase64String(bytes, 0, bytes.Length);
                            }
                        }
                    }
                    using (SqlCommand cmd = new("sp_actualizar_libros", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@IdLibro", SqlDbType.Int).Value = libro.IdLibro;
                        cmd.Parameters.Add("@Título", SqlDbType.VarChar).Value = libro.Título;
                        cmd.Parameters.Add("@Imagen", SqlDbType.VarChar).Value = i;
                        cmd.Parameters.Add("@Precio", SqlDbType.Decimal).Value = libro.Precio;
                        cmd.Parameters.Add("@Stock", SqlDbType.Int).Value = libro.Stock;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
            }
            catch (System.Exception e)
            {
                ViewBag.error = e.Message;
                return View();
            }
            return RedirectToAction("MantenimientoLibro");
        }


        public IActionResult Eliminar(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                Libro registro = new();
                using (SqlCommand cmd = new("sp_buscar_libro", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IdLibro", SqlDbType.Int).Value = id;
                    con.Open();

                    SqlDataAdapter da = new(cmd);
                    DataTable dt = new();
                    da.Fill(dt);
                    registro.IdLibro = (int)dt.Rows[0][0];
                    registro.Título = dt.Rows[0][1].ToString();
                    registro.Imagen = dt.Rows[0][2].ToString();
                    registro.Precio = (decimal)dt.Rows[0][3];
                    registro.Stock = (int)dt.Rows[0][4];
                }
                return View(registro);
            }
        }

        [HttpPost]
        public IActionResult Eliminar(Libro lib)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new("sp_eliminar_libros", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IdLibro", SqlDbType.Int).Value = lib.IdLibro;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                return RedirectToAction("MantenimientoLibro");
            }
        }
    }
}
