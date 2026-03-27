namespace Proyecto2Drones.Models
{
    public class ListaEnlazada<T>
    {
        private Nodo<T>? primero;
        private int conteo;

        public ListaEnlazada()
        {
            primero = null;
            conteo = 0;
        }

        public void Agregar(T nuevoDato)
        {
            Nodo<T> nuevoNodo = new Nodo<T>(nuevoDato);
            if (primero == null)
            {
                primero = nuevoNodo;
            }
            else
            {
                Nodo<T> actual = primero;
                while (actual.Siguiente != null)
                {
                    actual = actual.Siguiente;
                }
                actual.Siguiente = nuevoNodo;
            }
            conteo++;
        }

        public int GetConteo() => conteo;

        public T? Obtener(int indice)
        {
            if (indice < 0 || indice >= conteo || primero == null) return default;
            
            Nodo<T> actual = primero;
            for (int i = 0; i < indice; i++)
            {
                if (actual.Siguiente == null) break;
                actual = actual.Siguiente;
            }
            return actual.Dato;
        }

        public void OrdenarDrones()
        {
            if (conteo < 2 || primero == null) return;
            bool intercambio;
            do {
                intercambio = false;
                Nodo<T> actual = primero;
                while (actual != null && actual.Siguiente != null) {
                    if (actual.Dato is Dron d1 && actual.Siguiente.Dato is Dron d2) {
                        if (string.Compare(d1.Nombre, d2.Nombre) > 0) {
                            T temp = actual.Dato;
                            actual.Dato = actual.Siguiente.Dato;
                            actual.Siguiente.Dato = temp;
                            intercambio = true;
                        }
                    }
                    actual = actual.Siguiente;
                }
            } while (intercambio);
        }
    }
}