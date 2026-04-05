using Microsoft.AspNetCore.Mvc;
using Proyecto2Drones.Models;
using System.Text; 
using System.IO;

namespace Proyecto2Drones.Controllers
{
    public class DronController : Controller
    {
        private static LectorXML _lector = new LectorXML();

        [HttpGet]
        [Route("/")]             
        [Route("Dron/Index")]
        public IActionResult Index()
        {
            return View(_lector);
        }

        [HttpPost]
        public IActionResult CargarXML(IFormFile archivo)
        {
            if (archivo != null && archivo.Length > 0)
            {
                try
                {   
                    using (var reader = new StreamReader(archivo.OpenReadStream()))
                    {
                        string contenidoXml = reader.ReadToEnd();
                        _lector.CargarDesdeTexto(contenidoXml);
                        _lector.ProcesarMensajes();
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al procesar: " + ex.Message;
                }
            }
            return View("Index", _lector);
        }

        [HttpGet]
        public IActionResult VerGraficaSistemas()
        {
            string url = _lector.GenerarGrafoSistemas();
            
            if (string.IsNullOrEmpty(url))
            {
                TempData["Error"] = "No hay sistemas cargados para graficar.";
                return RedirectToAction("Index");
            }

            ViewBag.UrlGrafica = url;
            ViewBag.NombreMensaje = "Mapa Global de Sistemas y Alturas";
            return View("GraficaDetalle");
        }

        [HttpGet]
        public IActionResult VerGraficaMensaje(string nombre)
        {
            string url = _lector.GenerarGrafoInstrucciones(nombre);
            
            if (string.IsNullOrEmpty(url))
            {
                TempData["Error"] = "No se pudo generar la gráfica del mensaje.";
                return RedirectToAction("Index");
            }

            ViewBag.UrlGrafica = url;
            ViewBag.NombreMensaje = "Ruta Optimizada: " + nombre;
            return View("GraficaDetalle");
        }

        [HttpPost]
        public IActionResult AgregarDronManual(string nombreDron)
        {
            if (!string.IsNullOrEmpty(nombreDron))
            {
                bool existe = false;
                for (int i = 0; i < _lector.listaDronesGlobal.GetConteo(); i++)
                {
                    if (_lector.listaDronesGlobal.Obtener(i).Nombre.Equals(nombreDron, StringComparison.OrdinalIgnoreCase))
                    {
                        existe = true;
                        break;
                    }
                }

                if (!existe)
                {
                    _lector.listaDronesGlobal.Agregar(new Dron(nombreDron));
                    _lector.listaDronesGlobal.OrdenarDrones(); 
                }
                else
                {
                    TempData["Error"] = "El nombre del dron ya existe.";
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult DescargarXML()
        {
            if (_lector.listaResultados == null || _lector.listaResultados.GetConteo() == 0)
            {
                return RedirectToAction("Index");
            }

            StringBuilder xml = new StringBuilder();
            xml.AppendLine("<?xml version='1.0' encoding='utf8'?>");
            xml.AppendLine("<respuesta>");
            xml.AppendLine("    <listaMensajes>");

            for (int i = 0; i < _lector.listaResultados.GetConteo(); i++)
            {
                var res = _lector.listaResultados.Obtener(i);
                xml.AppendLine($"        <mensaje nombre=\"{res.NombreMensaje}\">");
                xml.AppendLine($"            <sistemaDrones>{res.NombreSistema}</sistemaDrones>");
                xml.AppendLine($"            <tiempoOptimo>{res.TiempoOptimo}</tiempoOptimo>");
                xml.AppendLine($"            <mensajeRecibido>{res.MensajeDecoded}</mensajeRecibido>");
                xml.AppendLine("            <instrucciones>");

                for (int j = 0; j < res.Pasos.GetConteo(); j++)
                {
                    var paso = res.Pasos.Obtener(j);
                    xml.AppendLine($"                <tiempo valor=\"{paso.Segundo}\">");
                    xml.AppendLine("                    <acciones>");
                    for (int k = 0; k < paso.Movimientos.GetConteo(); k++)
                    {
                        var mov = paso.Movimientos.Obtener(k);
                        xml.AppendLine($"                        <dron nombre=\"{mov.NombreDron}\">{mov.Accion}</dron>");
                    }
                    xml.AppendLine("                    </acciones>");
                    xml.AppendLine("                </tiempo>");
                }
                xml.AppendLine("            </instrucciones>");
                xml.AppendLine("        </mensaje>");
            }

            xml.AppendLine("    </listaMensajes>");
            xml.AppendLine("</respuesta>");

            var encoding = new UTF8Encoding(false);
            byte[] archivoBytes = encoding.GetBytes(xml.ToString());
            return File(archivoBytes, "application/xml", "Respuesta.xml");
        }

        [HttpGet]
        public IActionResult Ayuda()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Inicializar()
        {
            _lector = new LectorXML(); 
            return RedirectToAction("Index");
        }
    }
}