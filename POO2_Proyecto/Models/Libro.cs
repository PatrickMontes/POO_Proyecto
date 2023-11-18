using System.ComponentModel.DataAnnotations.Schema;

namespace POO2_Proyecto.Models
{
    public class Libro
    {
        public int IdLibro { get; set; }
        public string Título { get; set; }
        public string Imagen { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }

        [NotMapped]
        public IFormFile? File { get; set; }
    }
}
