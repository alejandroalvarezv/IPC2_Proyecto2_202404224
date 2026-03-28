using Microsoft.AspNetCore.Mvc;
using System.IO;
using Proyecto2Drones.Models; 

namespace Proyecto2Drones.Controllers 
{
    public class DronController : Controller
    {
        private static LectorXML _lector = new LectorXML();

        public IActionResult Index()
        {
            _lector.listaDronesGlobal.OrdenarDrones();
            return View(_lector);
        }

        [HttpPost]
        public IActionResult CargarXML(IFormFile archivo)
        {
            if (archivo != null && archivo.Length > 0)
            {
                var path = Path.GetTempFileName();
                using (var stream = System.IO.File.Create(path))
                {
                    archivo.CopyTo(stream);
                }
                _lector.CargarArchivo(path); 
            }
            return RedirectToAction("Index");
        }
    }
}