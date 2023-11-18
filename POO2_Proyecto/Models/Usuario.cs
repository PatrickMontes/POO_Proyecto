using System.ComponentModel.DataAnnotations;

namespace POO2_Proyecto.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Clave { get; set; }


        [Required()]public Rol IdRol { get; set; }
    }
}
