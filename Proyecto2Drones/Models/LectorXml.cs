using System;
using System.Xml;

namespace Proyecto2Drones.Models
{
    public class LectorXML
    {
        public ListaEnlazada<Dron> listaDronesGlobal { get; set; } = new ListaEnlazada<Dron>();
        public ListaEnlazada<SistemaDrones> listaSistemasDrones { get; set; } = new ListaEnlazada<SistemaDrones>();
        public ListaMensajes listaMensajesAProcesar { get; set; } = new ListaMensajes();
        public ListaEnlazada<RespuestaMensaje> listaResultados { get; set; } = new ListaEnlazada<RespuestaMensaje>();

        public void CargarDesdeTexto(string xmlPuro)
{
    try
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlPuro);

        // 1. Limpiar listas para evitar duplicados de cargas fallidas anteriores
        listaDronesGlobal = new ListaEnlazada<Dron>();
        listaSistemasDrones = new ListaEnlazada<SistemaDrones>();
        listaMensajesAProcesar = new ListaMensajes();
        listaResultados = new ListaEnlazada<RespuestaMensaje>();

        // 2. Leer Drones
        XmlNodeList drones = doc.GetElementsByTagName("dron");
        foreach (XmlNode nodo in drones)
        {
            if (nodo.ParentNode.Name == "listaDrones") // Solo los del inventario
                listaDronesGlobal.Agregar(new Dron(nodo.InnerText.Trim()));
        }

        // 3. Leer Sistemas (Búsqueda por nombre de etiqueta directamente)
        XmlNodeList sistemas = doc.GetElementsByTagName("sistemaDrones");
        foreach (XmlNode nodoSist in sistemas)
        {
            // Solo procesamos si tiene el atributo 'nombre' (para no confundir con la etiqueta hija)
            string nombre = nodoSist.Attributes["nombre"]?.Value;
            if (string.IsNullOrEmpty(nombre)) continue;

            int alt = int.Parse(nodoSist.SelectSingleNode("alturaMaxima")?.InnerText ?? "0");
            int cant = int.Parse(nodoSist.SelectSingleNode("cantidadDrones")?.InnerText ?? "0");

            SistemaDrones nuevoSist = new SistemaDrones(nombre, alt, cant);

            // Leer los drones configurados en este sistema
            XmlNodeList contenidos = nodoSist.SelectNodes("contenido");
            foreach (XmlNode cont in contenidos)
            {
                string dNombre = cont.SelectSingleNode("dron")?.InnerText ?? cont.SelectSingleNode("nombreDron")?.InnerText;
                ConfiguracionDron config = new ConfiguracionDron(dNombre?.Trim());

                XmlNodeList alturas = cont.SelectNodes("alturas/altura");
                foreach (XmlNode a in alturas)
                {
                    int v = int.Parse(a.Attributes["valor"]?.Value ?? "0");
                    config.ListadoAlturas.Agregar(new ContenidoAltura(v, a.InnerText.Trim()));
                }
                nuevoSist.DronesConfigurados.Agregar(config);
            }
            listaSistemasDrones.Agregar(nuevoSist);
            Console.WriteLine($"SISTEMA DETECTADO: {nombre}"); // Esto debe salir en consola
        }

        // 4. Leer Mensajes (Formato del nuevo XML)
        XmlNodeList mensajes = doc.GetElementsByTagName("Mensaje");
        foreach (XmlNode m in mensajes)
        {
            string id = m.Attributes["nombre"]?.Value;
            string sist = m.SelectSingleNode("sistemaDrones")?.InnerText.Trim();
            
            Mensaje nuevoMsj = new Mensaje(id, sist, "");

            XmlNodeList nodosInst = m.SelectNodes("instrucciones/instruccion");
            if (nodosInst != null)
            {
                foreach (XmlNode ins in nodosInst)
                {
                    string dronNom = ins.Attributes["dron"]?.Value;
                    if (int.TryParse(ins.InnerText.Trim(), out int altDestino))
                    {
                        nuevoMsj.AgregarInstruccion(dronNom, altDestino);
                    }
                }
            }
            listaMensajesAProcesar.Insertar(nuevoMsj);
        }

        Console.WriteLine($"---> CARGA FINALIZADA: {listaSistemasDrones.GetConteo()} sistemas en memoria.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("ERROR AL CARGAR: " + ex.Message);
    }
}

        public void ProcesarMensajes()
{
    for (int i = 0; i < listaMensajesAProcesar.GetConteo(); i++)
    {
        var msj = listaMensajesAProcesar.Obtener(i);
        if (msj == null) continue;

        RespuestaMensaje resultado = new RespuestaMensaje(msj.Nombre);
        int segundoActual = 1;

        // --- BUSQUEDA BLINDADA DEL SISTEMA ---
        SistemaDrones sistema = null;
        for (int j = 0; j < listaSistemasDrones.GetConteo(); j++)
        {
            var s = listaSistemasDrones.Obtener(j);
            // .Trim() elimina espacios y Equals con IgnoreCase ignora mayúsculas/minúsculas
            if (s != null && s.Nombre.Trim().Equals(msj.SistemaDrones.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                sistema = s;
                break;
            }
        }

        if (sistema == null) {
            Console.WriteLine($"---> ERROR CRÍTICO: El sistema '{msj.SistemaDrones}' no existe en la lista de sistemas cargados.");
            continue;
        }

        // Reiniciar drones a altura 0 para este mensaje
        for (int n = 0; n < listaDronesGlobal.GetConteo(); n++) {
            var d = listaDronesGlobal.Obtener(n);
            if (d != null) d.AlturaActual = 0;
        }

        // PROCESAR LAS INSTRUCCIONES DEL XML (El nuevo formato)
        for (int k = 0; k < msj.Instrucciones.GetConteo(); k++)
        {
            var instruccion = msj.Instrucciones.Obtener(k);
            if (instruccion == null) continue;

            bool objetivoAlcanzado = false;
            while (!objetivoAlcanzado)
            {
                PasoTiempo paso = new PasoTiempo(segundoActual);
                
                for (int m = 0; m < sistema.DronesConfigurados.GetConteo(); m++)
                {
                    var dronConf = sistema.DronesConfigurados.Obtener(m);
                    Dron dronReal = null;
                    
                    // Buscar el dron físico
                    for (int n = 0; n < listaDronesGlobal.GetConteo(); n++) {
                        var d = listaDronesGlobal.Obtener(n);
                        if (d != null && d.Nombre.Trim().Equals(dronConf.NombreDron.Trim(), StringComparison.OrdinalIgnoreCase)) {
                            dronReal = d;
                            break;
                        }
                    }

                    string accion = "Esperar";
                    if (dronReal != null)
                    {
                        // Si es el dron de la instrucción actual y aún no termina
                        if (dronReal.Nombre.Trim().Equals(instruccion.NombreDron.Trim(), StringComparison.OrdinalIgnoreCase) && !objetivoAlcanzado)
                        {
                            if (dronReal.AlturaActual < instruccion.AlturaDestino) {
                                accion = "Subir";
                                dronReal.AlturaActual++;
                            } else if (dronReal.AlturaActual > instruccion.AlturaDestino) {
                                accion = "Bajar";
                                dronReal.AlturaActual--;
                            } else {
                                accion = "Emitir Luz";
                                objetivoAlcanzado = true;
                            }
                        }
                    }
                    paso.Movimientos.Agregar(new InstruccionDron(dronConf.NombreDron, accion));
                }
                resultado.Pasos.Agregar(paso);
                segundoActual++;
            }
        }
        listaResultados.Agregar(resultado);
        Console.WriteLine($"---> EXITO: {msj.Nombre} procesado correctamente.");
    }
}
    }
}