using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace POO2_Proyecto.Models
{
    public class Pedido
    {
        [Display(Name = "DNI Cliente")] public string dniCliente { get; set; }
        [Display(Name = "Nombre Cliente")] public string nomCliente { get; set; }
        [Display(Name = "Email Cliente")] public string emailCliente { get; set; }
        [Display(Name = "Telefono")] public string telefono { get; set; }
    }
}
