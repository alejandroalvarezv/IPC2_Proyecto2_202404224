namespace Proyecto2Drones.Models
{
    
using System;
using System.Xml;

public class LectorXML
{
    public ListaEnlazada<Dron> listaDronesGlobal = new ListaEnlazada<Dron>();
    public ListaEnlazada<SistemaDrones> listaSistemasDrones = new ListaEnlazada<SistemaDrones>();

    public void CargarArchivo(string ruta)
    {
        try
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ruta);

            //Leer Lista de Drones (Configuración inicial)
            XmlNodeList drones = doc.SelectNodes("//listaDrones/dron");
            foreach (XmlNode nodoDron in drones)
            {
                string nombreDron = nodoDron.InnerText.Trim();
                listaDronesGlobal.Agregar(new Dron(nombreDron));
            }

            //Leer Sistemas de Drones
            XmlNodeList sistemas = doc.SelectNodes("//listaSistemasDrones/sistemaDrones");
            foreach (XmlNode nodoSist in sistemas)
            {
                string nombreSist = nodoSist.Attributes["nombre"].Value;
                int altMax = int.Parse(nodoSist.SelectSingleNode("alturaMaxima").InnerText);
                int cantDrones = int.Parse(nodoSist.SelectSingleNode("cantidadDrones").InnerText);

                SistemaDrones nuevoSistema = new SistemaDrones(nombreSist, altMax, cantDrones);

                // Leer contenido de cada sistema (Drones y sus alturas)
                XmlNodeList contenidos = nodoSist.SelectNodes("contenido");
                foreach (XmlNode cont in contenidos)
                {
                    string nombreDronSist = cont.SelectSingleNode("dron").InnerText.Trim();
                    ConfiguracionDron config = new ConfiguracionDron(nombreDronSist);

                    XmlNodeList alturas = cont.SelectNodes("alturas/altura");
                    foreach (XmlNode alt in alturas)
                    {
                        int valorAlt = int.Parse(alt.Attributes["valor"].Value);
                        string letra = alt.InnerText.Trim();
                        config.ListadoAlturas.Agregar(new ContenidoAltura(valorAlt, letra));
                    }
                    nuevoSistema.DronesConfigurados.Agregar(config);
                }
                listaSistemasDrones.Agregar(nuevoSistema);
            }

            Console.WriteLine("Archivo cargado exitosamente.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al leer el XML: " + ex.Message);
        }
    }
}
}