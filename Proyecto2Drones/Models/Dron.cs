namespace Proyecto2Drones.Models;

public class Dron
{
    public string Nombre { get; set; }
    
    public int AlturaActual { get; set; } = 0; 

    public Dron(string nombre)
    {
        Nombre = nombre;
        AlturaActual = 0; 
    }
}