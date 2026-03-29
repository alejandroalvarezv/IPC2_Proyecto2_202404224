namespace Proyecto2Drones.Models
{
    public class NodoMensaje
    {
        public Mensaje Dato { get; set; }
        public NodoMensaje Siguiente { get; set; }

        public NodoMensaje(Mensaje dato)
        {
            this.Dato = dato;
            this.Siguiente = null;
        }
    }
}