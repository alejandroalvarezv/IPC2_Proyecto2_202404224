namespace Proyecto2Drones.Models
{
    public class InstruccionDron
    {
        public string NombreDron { get; set; }
        public string Accion { get; set; } 

        public InstruccionDron(string nombre, string accion)
        {
            NombreDron = nombre;
            Accion     = accion;
        }
    }

    public class PasoTiempo
    {
        public int Segundo { get; set; }
        public ListaEnlazada<InstruccionDron> Movimientos { get; set; } = new ListaEnlazada<InstruccionDron>();

        public PasoTiempo(int segundo)
        {
            Segundo = segundo;
        }
    }

    public class RespuestaMensaje
    {
        public string NombreMensaje  { get; set; }
        public string NombreSistema  { get; set; }  
        public int    TiempoOptimo   { get; set; }  
        public string MensajeDecoded { get; set; }  

        public ListaEnlazada<PasoTiempo> Pasos { get; set; } = new ListaEnlazada<PasoTiempo>();

        public RespuestaMensaje(string nombre)
        {
            NombreMensaje  = nombre;
            NombreSistema  = "";
            TiempoOptimo   = 0;
            MensajeDecoded = "";
        }
    }
}