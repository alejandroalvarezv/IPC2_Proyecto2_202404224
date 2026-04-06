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

        public void Establecer(int indice, T valor)
        {
            if (indice < 0 || indice >= conteo || primero == null) return;
            Nodo<T> actual = primero;
            for (int i = 0; i < indice; i++)
            {
                if (actual.Siguiente == null) break;
                actual = actual.Siguiente;
            }
            actual.Dato = valor;
        }

        public void OrdenarDrones()
{
    if (conteo < 2 || primero == null) return;
    bool intercambio;
    do {
        intercambio = false;
        Nodo<T> actual = primero;
        while (actual != null && actual.Siguiente != null) {
            string nombre1 = "";
            string nombre2 = "";

            if (actual.Dato is Dron d1 && actual.Siguiente.Dato is Dron d2) {
                nombre1 = d1.Nombre;
                nombre2 = d2.Nombre;
            }
            else if (actual.Dato is ConfiguracionDron c1 && actual.Siguiente.Dato is ConfiguracionDron c2) {
                nombre1 = c1.NombreDron;
                nombre2 = c2.NombreDron;
            }

            if (!string.IsNullOrEmpty(nombre1) && string.Compare(nombre1, nombre2, StringComparison.OrdinalIgnoreCase) > 0) {
                T temp = actual.Dato;
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