namespace Proyecto2Drones.Models
{
public class SistemaDrones
{
    public string Nombre { get; set; }
    public int AlturaMaxima { get; set; } 
    public int CantidadDrones { get; set; } 
    
    public ListaEnlazada<ConfiguracionDron> DronesConfigurados { get; set; }

    public SistemaDrones(string nombre, int alturaMax, int cantidad)
    {
        Nombre = nombre;
        AlturaMaxima = alturaMax;
        CantidadDrones = cantidad;
        DronesConfigurados = new ListaEnlazada<ConfiguracionDron>();
    }
}

public class ConfiguracionDron
{
    public string NombreDron { get; set; }
    public ListaEnlazada<ContenidoAltura> ListadoAlturas { get; set; }

    public ConfiguracionDron(string nombre)
    {
        NombreDron = nombre;
        ListadoAlturas = new ListaEnlazada<ContenidoAltura>();
    }
}
}