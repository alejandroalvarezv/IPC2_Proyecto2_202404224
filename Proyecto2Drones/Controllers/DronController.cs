using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Proyecto2Drones.Models;
using System.IO; 

namespace Proyecto2Drones.Controllers;

public class DronController : Controller
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
            using (var reader = new StreamReader(archivo.OpenReadStream()))
            {
                string contenidoXml = reader.ReadToEnd();
                
                contenidoXml = contenidoXml.Trim();
                int firstTag = contenidoXml.IndexOf('<');
                if (firstTag >= 0) contenidoXml = contenidoXml.Substring(firstTag);

                modeloGlobal = new LectorXML(); 
                modeloGlobal.CargarDesdeTexto(contenidoXml);


                modeloGlobal.ProcesarMensajes(); 
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Error: " + ex.Message;
        }
    }
    
    return View("Index", modeloGlobal); 
}
}