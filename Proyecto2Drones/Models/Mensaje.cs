namespace Proyecto2Drones.Models
{
    public class Mensaje
    {
        public string Nombre { get; set; }
        public string SistemaDrones { get; set; }
        public string Contenido { get; set; }
        public ListaEnlazada<InstruccionDirecta> Instrucciones { get; set; } = new ListaEnlazada<InstruccionDirecta>();

        public Mensaje(string nombre, string sistema, string contenido)
        {
            Nombre = nombre;
            SistemaDrones = sistema;
            Contenido = contenido;
        }

        public void AgregarInstruccion(string dron, int altura)
        {
            Instrucciones.Agregar(new InstruccionDirecta(dron, altura));
        }
    }

    public class InstruccionDirecta
    {
        public string NombreDron { get; set; }
        public int AlturaDestino { get; set; }
        public InstruccionDirecta(string dron, int alt) { NombreDron = dron; AlturaDestino = alt; }
    }
}