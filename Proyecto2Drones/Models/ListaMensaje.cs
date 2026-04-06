namespace Proyecto2Drones.Models
{
    public class ListaMensajes
    {
        private NodoMensaje cabeza;
        private int conteo;

        public ListaMensajes()
        {
            cabeza = null;
            conteo = 0;
        }

        public void Insertar(Mensaje nuevoMensaje)
        {
            NodoMensaje nuevoNodo = new NodoMensaje(nuevoMensaje);
            if (cabeza == null)
            {
                cabeza = nuevoNodo;
            }
            else
            {
                NodoMensaje actual = cabeza;
                while (actual.Siguiente != null)
                {
                    actual = actual.Siguiente;
                }
                actual.Siguiente = nuevoNodo;
            }
            conteo++;
        }

        public int GetConteo() => conteo;

        public Mensaje Obtener(int indice)
        {
            NodoMensaje actual = cabeza;
            int contador = 0;
            while (actual != null)
            {
                if (contador == indice) return actual.Dato;
                actual = actual.Siguiente;
                contador++;
            }
            return null;
        }

        public void Ordenar()
        {
            if (conteo < 2 || cabeza == null) return;
            bool intercambio;
            do
            {
                intercambio = false;
                NodoMensaje actual = cabeza;
                while (actual != null && actual.Siguiente != null)
                {
                    if (string.Compare(actual.Dato.Nombre, actual.Siguiente.Dato.Nombre, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        Mensaje temp = actual.Dato;
                        actual.Dato = actual.Siguiente.Dato;
                        actual.Siguiente.Dato = temp;
                        intercambio = true;
                    }
                    actual = actual.Siguiente;
                }
            } while (intercambio);
        }
    }
}