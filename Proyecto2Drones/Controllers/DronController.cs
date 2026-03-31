using Microsoft.AspNetCore.Mvc;
using Proyecto2Drones.Models;

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
                    _lector = new LectorXML(); // Limpieza total
                    using (var reader = new StreamReader(archivo.OpenReadStream()))
                    {
                        string contenidoXml = reader.ReadToEnd();
                        _lector.CargarDesdeTexto(contenidoXml);
                        _lector.ProcesarMensajes();
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Error crítico: " + ex.Message;
                }
            }
            return View("Index", _lector);
        }
    }
}