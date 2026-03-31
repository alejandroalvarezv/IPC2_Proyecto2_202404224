using System;
using System.Collections.Generic;
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

                listaDronesGlobal = new ListaEnlazada<Dron>();
                listaSistemasDrones = new ListaEnlazada<SistemaDrones>();
                listaMensajesAProcesar = new ListaMensajes();

                // 1. Inventario de drones globales
                XmlNodeList drones = doc.GetElementsByTagName("dron");
                foreach (XmlNode nodo in drones)
                {
                    if (nodo.ParentNode.Name == "listaDrones")
                        listaDronesGlobal.Agregar(new Dron(nodo.InnerText.Trim()));
                }

                // 2. Sistemas de drones
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

                // 3. Mensajes
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error XML: " + ex.Message);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // ALGORITMO CORRECTO
        //
        // Regla clave: solo UN dron puede emitir luz en un tiempo t dado.
        // Las instrucciones definen el ORDEN GLOBAL de emisiones.
        //
        // Fórmula:
        //   emitTime = max(droneFreeAt[d] + travel, lastGlobalEmit + 1)
        //
        // Donde:
        //   droneFreeAt[d]  = primer segundo en que el dron puede actuar
        //                     (= 1 al inicio; = emitTime_anterior + 1 tras cada emisión)
        //   travel          = |alturaActual - alturaDestino|
        //   lastGlobalEmit  = tiempo en que ocurrió la última emisión global
        // ─────────────────────────────────────────────────────────────────────
        public void ProcesarMensajes()
        {
            listaResultados = new ListaEnlazada<RespuestaMensaje>();

            for (int msgIdx = 0; msgIdx < listaMensajesAProcesar.GetConteo(); msgIdx++)
            {
                Mensaje msj = listaMensajesAProcesar.Obtener(msgIdx);

                // Buscar el sistema de drones asociado al mensaje
                SistemaDrones sistemaActual = null;
                for (int s = 0; s < listaSistemasDrones.GetConteo(); s++)
                {
                    SistemaDrones candidato = listaSistemasDrones.Obtener(s);
                    if (candidato.Nombre.Trim().Equals(msj.SistemaDrones.Trim(),
                            StringComparison.OrdinalIgnoreCase))
                    {
                        sistemaActual = candidato;
                        break;
                    }
                }
                if (sistemaActual == null) continue;

                int n = sistemaActual.DronesConfigurados.GetConteo();

                // Mapeo nombre-dron → índice en el sistema
                Dictionary<string, int> droneIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int d = 0; d < n; d++)
                    droneIndex[sistemaActual.DronesConfigurados.Obtener(d).NombreDron.Trim()] = d;

                // Estado inicial de cada dron
                int[] currentHeight = new int[n];   // altura actual (empieza en 0)
                int[] droneFreeAt   = new int[n];   // primer segundo disponible
                for (int i = 0; i < n; i++) droneFreeAt[i] = 1;

                int lastGlobalEmit = 0;

                // ── Fase 1: calcular tiempos de emisión ──────────────────────
                int totalInstrucciones = msj.Instrucciones.GetConteo();

                int[]    instrDroneIdx   = new int[totalInstrucciones];
                int[]    instrTargetH    = new int[totalInstrucciones];
                int[]    instrMoveStart  = new int[totalInstrucciones];
                int[]    instrEmitTime   = new int[totalInstrucciones];
                int[]    instrPrevHeight = new int[totalInstrucciones];
                string[] instrLetras     = new string[totalInstrucciones];

                for (int i = 0; i < totalInstrucciones; i++)
                {
                    InstruccionDirecta inst = msj.Instrucciones.Obtener(i);
                    string dronNombre = inst.NombreDron.Trim();

                    if (!droneIndex.ContainsKey(dronNombre))
                    {
                        // Dron no pertenece al sistema → ignorar instrucción
                        instrDroneIdx[i]   = -1;
                        instrLetras[i]     = "?";
                        continue;
                    }

                    int d      = droneIndex[dronNombre];
                    int target = inst.AlturaDestino;
                    int prevH  = currentHeight[d];
                    int travel = Math.Abs(prevH - target);

                    int moveStart    = droneFreeAt[d];
                    int earliestEmit = moveStart + travel;
                    int emitTime     = Math.Max(earliestEmit, lastGlobalEmit + 1);

                    instrDroneIdx[i]   = d;
                    instrTargetH[i]    = target;
                    instrMoveStart[i]  = moveStart;
                    instrEmitTime[i]   = emitTime;
                    instrPrevHeight[i] = prevH;

                    // Decodificar letra
                    ConfiguracionDron config = sistemaActual.DronesConfigurados.Obtener(d);
                    instrLetras[i] = "?";
                    for (int h = 0; h < config.ListadoAlturas.GetConteo(); h++)
                    {
                        ContenidoAltura ca = config.ListadoAlturas.Obtener(h);
                        if (ca.ValorAltura == target) { instrLetras[i] = ca.Letra; break; }
                    }

                    // Actualizar estado
                    currentHeight[d] = target;
                    droneFreeAt[d]   = emitTime + 1;
                    lastGlobalEmit   = emitTime;
                }

                int tiempoOptimo = lastGlobalEmit;

                // ── Fase 2: construir tabla de acciones por segundo ───────────
                // Para cada dron y cada segundo t ∈ [1, tiempoOptimo]:
                //   · Subir / Bajar  durante los segundos de desplazamiento
                //   · Esperar        si está en destino pero aún no es su turno
                //   · Emitir Luz     en el segundo exacto de emisión

                // Usamos Dictionary<int,string> por dron para asignar acciones
                Dictionary<int, string>[] droneActions = new Dictionary<int, string>[n];
                for (int d = 0; d < n; d++)
                    droneActions[d] = new Dictionary<int, string>();

                for (int i = 0; i < totalInstrucciones; i++)
                {
                    int d = instrDroneIdx[i];
                    if (d < 0) continue; // instrucción ignorada

                    int target    = instrTargetH[i];
                    int prevH     = instrPrevHeight[i];
                    int moveStart = instrMoveStart[i];
                    int emitTime  = instrEmitTime[i];
                    int travel    = Math.Abs(prevH - target);
                    int direction = (target > prevH) ? 1 : (target < prevH ? -1 : 0);

                    // Segundos de movimiento
                    for (int t = moveStart; t < moveStart + travel; t++)
                    {
                        string accion = (direction > 0) ? "Subir" : "Bajar";
                        if (!droneActions[d].ContainsKey(t))
                            droneActions[d][t] = accion;
                    }

                    // Segundos de espera en destino (antes de que llegue el turno global)
                    for (int t = moveStart + travel; t < emitTime; t++)
                    {
                        if (!droneActions[d].ContainsKey(t))
                            droneActions[d][t] = "Esperar";
                    }

                    // Segundo de emisión
                    if (!droneActions[d].ContainsKey(emitTime))
                        droneActions[d][emitTime] = "Emitir Luz";
                }

                // Rellenar con "Esperar" cualquier segundo no cubierto
                for (int t = 1; t <= tiempoOptimo; t++)
                    for (int d = 0; d < n; d++)
                        if (!droneActions[d].ContainsKey(t))
                            droneActions[d][t] = "Esperar";

                // ── Fase 3: construir el objeto de respuesta ─────────────────
                RespuestaMensaje r = new RespuestaMensaje(msj.Nombre);
                r.NombreSistema  = msj.SistemaDrones;
                r.TiempoOptimo   = tiempoOptimo;

                // Construir mensaje decodificado
                System.Text.StringBuilder sbMensaje = new System.Text.StringBuilder();
                for (int i = 0; i < totalInstrucciones; i++)
                    sbMensaje.Append(instrLetras[i]);
                r.MensajeDecoded = sbMensaje.ToString();

                // Generar un PasoTiempo para CADA segundo (incluyendo movimientos y esperas)
                for (int t = 1; t <= tiempoOptimo; t++)
                {
                    PasoTiempo paso = new PasoTiempo(t);
                    for (int d = 0; d < n; d++)
                    {
                        string nombreDron = sistemaActual.DronesConfigurados.Obtener(d).NombreDron;
                        string accion     = droneActions[d].ContainsKey(t) ? droneActions[d][t] : "Esperar";
                        paso.Movimientos.Agregar(new InstruccionDron(nombreDron, accion));
                    }
                    r.Pasos.Agregar(paso);
                }

                listaResultados.Agregar(r);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // Genera el XML de salida según el formato requerido
        // ─────────────────────────────────────────────────────────────────────
        public string GenerarXmlSalida()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<respuesta>");
            sb.AppendLine("  <listaMensajes>");

            for (int i = 0; i < listaResultados.GetConteo(); i++)
            {
                RespuestaMensaje rm = listaResultados.Obtener(i);
                sb.AppendLine($"    <mensaje nombre=\"{rm.NombreMensaje}\">");
                sb.AppendLine($"      <sistemaDrones>{rm.NombreSistema}</sistemaDrones>");
                sb.AppendLine($"      <tiempoOptimo>{rm.TiempoOptimo}</tiempoOptimo>");
                sb.AppendLine($"      <mensajeRecibido>{rm.MensajeDecoded}</mensajeRecibido>");
                sb.AppendLine("      <instrucciones>");

                for (int p = 0; p < rm.Pasos.GetConteo(); p++)
                {
                    PasoTiempo paso = rm.Pasos.Obtener(p);
                    sb.AppendLine($"        <tiempo valor=\"{paso.Segundo}\">");
                    sb.AppendLine("          <acciones>");
                    for (int m = 0; m < paso.Movimientos.GetConteo(); m++)
                    {
                        InstruccionDron mov = paso.Movimientos.Obtener(m);
                        sb.AppendLine($"            <dron nombre=\"{mov.NombreDron}\">{mov.Accion}</dron>");
                    }
                    sb.AppendLine("          </acciones>");
                    sb.AppendLine("        </tiempo>");
                }

                sb.AppendLine("      </instrucciones>");
                sb.AppendLine("    </mensaje>");
            }

            sb.AppendLine("  </listaMensajes>");
            sb.AppendLine("</respuesta>");
            return sb.ToString();
        }
    }
}