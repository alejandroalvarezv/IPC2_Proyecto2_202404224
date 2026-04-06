namespace Proyecto2Drones.Models
{
    // Estado de cada dron durante la simulación (reemplaza int[] currentHeight y int[] droneFreeAt)
    public class EstadoDron
    {
        public int AlturaActual { get; set; } = 0;
        public int LibreEn { get; set; } = 1;
    }

    // Datos calculados por instrucción (reemplaza los 5 int[] y el string[] de instrucciones)
    public class DatosInstruccion
    {
        public int IndiceDron { get; set; }
        public int AlturaDestino { get; set; }
        public int InicioMovimiento { get; set; }
        public int TiempoEmision { get; set; }
        public int AlturaAnterior { get; set; }
        public string Letra { get; set; } = "?";
    }

    // TDA propio para reemplazar string[,] matrizAcciones
    public class MatrizAcciones
    {
        private ListaEnlazada<ListaEnlazada<string>> filas;

        public MatrizAcciones(int numDrones, int tiempoOptimo)
        {
            filas = new ListaEnlazada<ListaEnlazada<string>>();
            for (int d = 0; d < numDrones; d++)
            {
                ListaEnlazada<string> fila = new ListaEnlazada<string>();
                for (int t = 0; t <= tiempoOptimo; t++)
                    fila.Agregar("Esperar");
                filas.Agregar(fila);
            }
        }

        public string Obtener(int dron, int tiempo)
        {
            ListaEnlazada<string> fila = filas.Obtener(dron);
            if (fila == null) return "Esperar";
            return fila.Obtener(tiempo) ?? "Esperar";
        }

        public void Establecer(int dron, int tiempo, string valor)
        {
            ListaEnlazada<string> fila = filas.Obtener(dron);
            if (fila != null)
                fila.Establecer(tiempo, valor);
        }
    }
}