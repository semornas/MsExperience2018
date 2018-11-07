using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MsExperience.DevAzure.Front.Helpers;
using MsExperience.DevAzure.Front.Models;

namespace MsExperience.DevAzure.Front.Controllers
{
    public class HomeController : Controller
    {
        private readonly AzureStorageConfig storageConfig = null;

        public HomeController(IOptions<AzureStorageConfig> config)
        {
            storageConfig = config.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Stats()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewData["message"] = "Veuillez choisir un fichier";
                return View();
            }

            try
            {
                if (StorageHelper.IsImage(file))
                {
                    if (file.Length > 0)
                    {
                        using (Stream stream = file.OpenReadStream())
                        {
                            await StorageHelper.UploadFileToStorage(stream, file.FileName, storageConfig, file.ContentType);
                            ViewData["message"] = "Fichier uploadé";
                            ViewData["success"] = true;
                        }
                    }
                }
                else
                {
                    ViewData["message"] = "Le fichier n'est pas une image";
                }
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
            }
             
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}