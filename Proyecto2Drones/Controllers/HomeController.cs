using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Proyecto2Drones.Models;

namespace Proyecto2Drones.Controllers;

public class HomeController : Controller
{
    private static LectorXML modeloGlobal = new LectorXML();

    public IActionResult Index()
    {
        return View(modeloGlobal);
    }

    [HttpPost]
    public IActionResult CargarXML(IFormFile archivo)
    {
        if (archivo != null && archivo.Length > 0)
        {
            try
            {
                modeloGlobal = new LectorXML();

                using (var reader = new StreamReader(archivo.OpenReadStream()))
                {
                    string contenidoXml = reader.ReadToEnd();
                    modeloGlobal.CargarDesdeTexto(contenidoXml);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al procesar: " + ex.Message;
            }
        }
        return View("Index", modeloGlobal);
    }

    public IActionResult Privacy() => View();
}