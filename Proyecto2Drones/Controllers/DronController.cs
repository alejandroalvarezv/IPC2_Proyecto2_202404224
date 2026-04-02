using Microsoft.AspNetCore.Mvc;
using Proyecto2Drones.Models;
using System.Text; 
using System.IO;

namespace Proyecto2Drones.Controllers
{
    public class DronController : Controller
    {
        private static LectorXML _lector = new LectorXML();

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
                    _lector = new LectorXML(); 
                    
                    using (var reader = new StreamReader(archivo.OpenReadStream()))
                    {
                        string contenidoXml = reader.ReadToEnd();
                        _lector.CargarDesdeTexto(contenidoXml);
                        
                        if (_lector.listaDronesGlobal != null)
                        {
                            _lector.listaDronesGlobal.OrdenarDrones();
                        }
                        
                        _lector.ProcesarMensajes();
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error al procesar el archivo: " + ex.Message;
                }
            }
            return View("Index", _lector);
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
                xml.AppendLine("            <sistemaDrones>SistemaComida</sistemaDrones>");
                xml.AppendLine($"            <tiempoOptimo>{res.Pasos.GetConteo()}</tiempoOptimo>");
                xml.AppendLine($"            <mensajeRecibido>{res.NombreMensaje}</mensajeRecibido>");
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
    }
}