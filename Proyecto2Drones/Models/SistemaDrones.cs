namespace Proyecto2Drones.Models
{
    public class SistemaDrones
    {
        public string Nombre { get; set; }
        public int AlturaMaxima { get; set; } 
        public int CantidadDrones { get; set; } 
        
        // Debe tener get y set para que el Lector pueda leerlo
        public ListaEnlazada<ConfiguracionDron> DronesConfigurados { get; set; } = new ListaEnlazada<ConfiguracionDron>();

        public SistemaDrones(string nombre, int alturaMax, int cantidad)
        {
            Nombre = nombre;
            AlturaMaxima = alturaMax;
            CantidadDrones = cantidad;
        }
    }

    public class ConfiguracionDron
    {
        public string NombreDron { get; set; }
        public ListaEnlazada<ContenidoAltura> ListadoAlturas { get; set; } = new ListaEnlazada<ContenidoAltura>();

        public ConfiguracionDron(string nombre)
        {
            NombreDron = nombre;
        }
    }
}