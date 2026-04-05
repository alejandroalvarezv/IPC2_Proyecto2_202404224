using System;
using System.Xml;
using System.Text; 

namespace Proyecto2Drones.Models
{
    public class LectorXML
    {
        public ListaEnlazada<Dron> listaDronesGlobal { get; set; } = new ListaEnlazada<Dron>();
        public ListaEnlazada<SistemaDrones> listaSistemasDrones { get; set; } = new ListaEnlazada<SistemaDrones>();
        public ListaMensajes listaMensajesAProcesar { get; set; } = new ListaMensajes();
        public ListaEnlazada<RespuestaMensaje> listaResultados { get; set; } = new ListaEnlazada<RespuestaMensaje>();

        // Método auxiliar para evitar usar Dictionary nativo (Restricción TDA)
        private int BuscarIndiceDron(SistemaDrones sistema, string nombreDron)
        {
            for (int i = 0; i < sistema.DronesConfigurados.GetConteo(); i++)
            {
                if (sistema.DronesConfigurados.Obtener(i).NombreDron.Trim().Equals(nombreDron.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        public void CargarDesdeTexto(string xmlPuro)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlPuro);

                XmlNodeList drones = doc.GetElementsByTagName("dron");
                foreach (XmlNode nodo in drones)
                {
                    if (nodo.ParentNode.Name == "listaDrones")
                        listaDronesGlobal.Agregar(new Dron(nodo.InnerText.Trim()));
                }

                XmlNodeList sistemas = doc.GetElementsByTagName("sistemaDrones");
                foreach (XmlNode nodoSist in sistemas)
                {
                    string nombreSist = (nodoSist.Attributes["nombre"]?.Value ?? "Sistema").Trim();
                    int altMax = int.Parse(nodoSist.SelectSingleNode("alturaMaxima")?.InnerText ?? "0");
                    int cantDrones = int.Parse(nodoSist.SelectSingleNode("cantidadDrones")?.InnerText ?? "0");

                    SistemaDrones nuevoSist = new SistemaDrones(nombreSist, altMax, cantDrones);

                    XmlNodeList contenidos = nodoSist.SelectNodes("contenido");
                    foreach (XmlNode cont in contenidos)
                    {
                        string nombreDr = (cont.SelectSingleNode("dron")?.InnerText
                                        ?? cont.SelectSingleNode("nombreDron")?.InnerText)?.Trim();
                        ConfiguracionDron config = new ConfiguracionDron(nombreDr);

                        XmlNodeList alturas = cont.SelectNodes("alturas/altura");
                        foreach (XmlNode a in alturas)
                        {
                            int val = int.Parse(a.Attributes["valor"]?.Value ?? "0");
                            config.ListadoAlturas.Agregar(new ContenidoAltura(val, a.InnerText.Trim()));
                        }
                        nuevoSist.DronesConfigurados.Agregar(config);
                    }
                    listaSistemasDrones.Agregar(nuevoSist);
                }

                XmlNodeList mensajes = doc.GetElementsByTagName("Mensaje");
                if (mensajes.Count == 0) mensajes = doc.GetElementsByTagName("mensaje");

                foreach (XmlNode m in mensajes)
                {
                    string id = (m.Attributes["nombre"]?.Value ?? "Msg").Trim();
                    string sistRef = m.SelectSingleNode("sistemaDrones")?.InnerText.Trim();
                    Mensaje nuevoMsj = new Mensaje(id, sistRef, "");

                    XmlNodeList instrucciones = m.SelectNodes("instrucciones/instruccion");
                    foreach (XmlNode ins in instrucciones)
                    {
                        string dNom = ins.Attributes["dron"]?.Value?.Trim();
                        int aDest = int.Parse(ins.InnerText.Trim());
                        nuevoMsj.AgregarInstruccion(dNom, aDest);
                    }
                    listaMensajesAProcesar.Insertar(nuevoMsj);
                }

                listaDronesGlobal.OrdenarDrones();

                for (int i = 0; i < listaSistemasDrones.GetConteo(); i++)
                {
                    var sistema = listaSistemasDrones.Obtener(i);
                    if (sistema != null && sistema.DronesConfigurados != null)
                    {
                        sistema.DronesConfigurados.OrdenarDrones();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error XML: " + ex.Message);
            }
        }

        public void ProcesarMensajes()
        {
            listaResultados = new ListaEnlazada<RespuestaMensaje>();

            for (int msgIdx = 0; msgIdx < listaMensajesAProcesar.GetConteo(); msgIdx++)
            {
                Mensaje msj = listaMensajesAProcesar.Obtener(msgIdx);
                SistemaDrones sistemaActual = null;

                for (int s = 0; s < listaSistemasDrones.GetConteo(); s++)
                {
                    SistemaDrones candidato = listaSistemasDrones.Obtener(s);
                    if (candidato.Nombre.Trim().Equals(msj.SistemaDrones.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        sistemaActual = candidato;
                        break;
                    }
                }
                if (sistemaActual == null) continue;

                int n = sistemaActual.DronesConfigurados.GetConteo();
                int[] currentHeight = new int[n];   
                int[] droneFreeAt = new int[n];   
                for (int i = 0; i < n; i++) droneFreeAt[i] = 1;

                int lastGlobalEmit = 0;
                int totalInstrucciones = msj.Instrucciones.GetConteo();

                int[] instrDroneIdx = new int[totalInstrucciones];
                int[] instrTargetH = new int[totalInstrucciones];
                int[] instrMoveStart = new int[totalInstrucciones];
                int[] instrEmitTime = new int[totalInstrucciones];
                int[] instrPrevHeight = new int[totalInstrucciones];
                string[] instrLetras = new string[totalInstrucciones];

                // Simulación de Tiempos
                for (int i = 0; i < totalInstrucciones; i++)
                {
                    InstruccionDirecta inst = msj.Instrucciones.Obtener(i);
                    string dronNombre = inst.NombreDron.Trim();

                    int d = BuscarIndiceDron(sistemaActual, dronNombre);
                    if (d == -1)
                    {
                        instrDroneIdx[i] = -1;
                        instrLetras[i] = "?";
                        continue;
                    }

                    int target = inst.AlturaDestino;
                    int prevH = currentHeight[d];
                    int travel = Math.Abs(prevH - target);

                    int moveStart = droneFreeAt[d];
                    int earliestEmit = moveStart + travel;
                    int emitTime = Math.Max(earliestEmit, lastGlobalEmit + 1);

                    instrDroneIdx[i] = d;
                    instrTargetH[i] = target;
                    instrMoveStart[i] = moveStart;
                    instrEmitTime[i] = emitTime;
                    instrPrevHeight[i] = prevH;

                    ConfiguracionDron config = sistemaActual.DronesConfigurados.Obtener(d);
                    instrLetras[i] = "?";
                    for (int h = 0; h < config.ListadoAlturas.GetConteo(); h++)
                    {
                        ContenidoAltura ca = config.ListadoAlturas.Obtener(h);
                        if (ca.ValorAltura == target) { instrLetras[i] = ca.Letra; break; }
                    }

                    currentHeight[d] = target;
                    droneFreeAt[d] = emitTime + 1;
                    lastGlobalEmit = emitTime;
                }

                int tiempoOptimo = lastGlobalEmit;

                string[,] matrizAcciones = new string[n, tiempoOptimo + 1];
                for (int d = 0; d < n; d++)
                    for (int t = 0; t <= tiempoOptimo; t++)
                        matrizAcciones[d, t] = "Esperar";

                for (int i = 0; i < totalInstrucciones; i++)
                {
                    int d = instrDroneIdx[i];
                    if (d < 0) continue;

                    int target = instrTargetH[i];
                    int prevH = instrPrevHeight[i];
                    int moveStart = instrMoveStart[i];
                    int emitTime = instrEmitTime[i];
                    int travel = Math.Abs(prevH - target);
                    int direction = (target > prevH) ? 1 : (target < prevH ? -1 : 0);

                    for (int t = moveStart; t < moveStart + travel; t++)
                        matrizAcciones[d, t] = (direction > 0) ? "Subir" : "Bajar";

                    matrizAcciones[d, emitTime] = "Emitir Luz";
                }

                RespuestaMensaje r = new RespuestaMensaje(msj.Nombre);
                r.NombreSistema = msj.SistemaDrones;
                r.TiempoOptimo = tiempoOptimo;

                StringBuilder sbMensaje = new StringBuilder();
                for (int i = 0; i < totalInstrucciones; i++)
                    sbMensaje.Append(instrLetras[i]);
                r.MensajeDecoded = sbMensaje.ToString();

                for (int t = 1; t <= tiempoOptimo; t++)
                {
                    PasoTiempo paso = new PasoTiempo(t);
                    for (int d = 0; d < n; d++)
                    {
                        string nombreDron = sistemaActual.DronesConfigurados.Obtener(d).NombreDron;
                        paso.Movimientos.Agregar(new InstruccionDron(nombreDron, matrizAcciones[d, t]));
                    }
                    r.Pasos.Agregar(paso);
                }
                listaResultados.Agregar(r);
            }
        }

        public string GenerarGrafoSistemas()
        {
            if (listaSistemasDrones == null || listaSistemasDrones.GetConteo() == 0) return "";
            StringBuilder dot = new StringBuilder();
            dot.Append("digraph G { rankdir=LR; node [shape=box, style=filled, color=lightblue]; ");
            dot.Append("Raiz [label=\"Sistemas Cargados\", shape=ellipse, color=orange]; ");

            for (int i = 0; i < listaSistemasDrones.GetConteo(); i++)
            {
                var sistema = listaSistemasDrones.Obtener(i);
                dot.Append($"Raiz -> \"{sistema.Nombre}\"; ");
                dot.Append($"\"{sistema.Nombre}\" [label=\"{sistema.Nombre}\\nAltura Máx: {sistema.AlturaMaxima}m\"]; ");
            }
            dot.Append(" }");
            return $"https://quickchart.io/graphviz?graph={Uri.EscapeDataString(dot.ToString())}";
        }

        public string GenerarGrafoInstrucciones(string nombreMensaje)
        {
            RespuestaMensaje res = null;
            for (int i = 0; i < listaResultados.GetConteo(); i++) {
                if (listaResultados.Obtener(i).NombreMensaje == nombreMensaje) {
                    res = listaResultados.Obtener(i);
                    break;
                }
            }
            if (res == null) return "";

            StringBuilder dot = new StringBuilder();
            dot.Append("digraph G { node [shape=record, fontname=\"Arial\"]; ");
            dot.Append($"Header [label=\"{{MENSAJE: {res.NombreMensaje} | TIEMPO TOTAL: {res.TiempoOptimo}s}}\", style=filled, color=gold]; ");
            string ultimoNodo = "Header";

            for (int j = 0; j < res.Pasos.GetConteo(); j++)
            {
                var paso = res.Pasos.Obtener(j);
                string idNodo = $"Paso{j}";
                string label = $"{{ Segundo: {paso.Segundo} | {{ ";
                for (int k = 0; k < paso.Movimientos.GetConteo(); k++)
                {
                    var mov = paso.Movimientos.Obtener(k);
                    label += $"{mov.NombreDron}: {mov.Accion}";
                    if (k < paso.Movimientos.GetConteo() - 1) label += " | ";
                }
                label += " }} }}";
                dot.Append($"{idNodo} [label=\"{label}\"]; {ultimoNodo} -> {idNodo}; ");
                ultimoNodo = idNodo;
            }
            dot.Append("}");
            return $"https://quickchart.io/graphviz?graph={Uri.EscapeDataString(dot.ToString())}";
        }
    }
}