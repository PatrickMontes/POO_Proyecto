using Microsoft.AspNetCore.Mvc;
using POO2_Proyecto.Models;
using POO2_Proyecto.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace POO2_Proyecto.Controllers
{
    public class AccesoController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccesoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(Usuario _usuario)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            DA_Usuario _da_usuario = new DA_Usuario(connectionString);

            var usuario = _da_usuario.ValidarUsuario(_usuario.Correo, _usuario.Clave);

            if (usuario != null)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim("Correo", usuario.Correo),
            new Claim(ClaimTypes.Role, usuario.IdRol.ToString())
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }


        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
    }
}
