namespace POO2_Proyecto.Models
{
    public class Registro
    {
        public int IdRegistro { get; set; }
        public string Título { get; set; }
        public string Imagen { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public decimal Monto { get { return Precio * Cantidad; } }

    }
}
