using System;

namespace Proyecto2Drones.Models
{
    // Representa lo que hace UN dron en UN segundo específico
    public class InstruccionDron
    {
        public string NombreDron { get; set; }
        public string Accion { get; set; } // "Subir", "Bajar", "Esperar", "Emitir Luz"

        public InstruccionDron(string nombre, string accion)
        {
            NombreDron = nombre;
            Accion = accion;
        }
    }

    // Representa un segundo de tiempo con todos los drones actuando
    public class PasoTiempo
    {
        public int Segundo { get; set; }
        public ListaEnlazada<InstruccionDron> Movimientos { get; set; } = new ListaEnlazada<InstruccionDron>();

        public PasoTiempo(int segundo)
        {
            Segundo = segundo;
        }
    }

    // El resultado final de procesar un mensaje completo
    public class RespuestaMensaje
    {
        public string NombreMensaje { get; set; }
        public ListaEnlazada<PasoTiempo> Pasos { get; set; } = new ListaEnlazada<PasoTiempo>();

        public RespuestaMensaje(string nombre)
        {
            NombreMensaje = nombre;
        }
    }
}