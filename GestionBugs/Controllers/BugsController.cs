using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using GestionBugs.Models;

namespace GestionBugs.Controllers
{
    public class BugsController : Controller
    {
        private const int TailleMaxCapture = 5 * 1024 * 1024;
        private static readonly string[] Priorites = { "Faible", "Normale", "Haute", "Critique" };

        [HttpGet]
        public ActionResult Creer()
        {
            return View(new BugDeclarationViewModel { Priorite = "Normale" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Creer(BugDeclarationViewModel modele)
        {
            if (!Priorites.Contains(modele.Priorite))
            {
                ModelState.AddModelError("Priorite", "Veuillez choisir une priorité valide.");
            }

            var fichier = modele.CaptureEcran;
            var captureSelectionnee = fichier != null && !string.IsNullOrWhiteSpace(fichier.FileName);
            ViewBag.CaptureAReselectionner = captureSelectionnee;

            if (captureSelectionnee)
            {
                ValiderCapture(fichier);
            }

            if (!ModelState.IsValid)
            {
                return View(modele);
            }

            ViewBag.MessageSucces = "Formulaire validé — enregistrement non encore connecté";
            return View(modele);
        }

        private void ValiderCapture(System.Web.HttpPostedFileBase fichier)
        {
            if (fichier.ContentLength <= 0)
            {
                ModelState.AddModelError("CaptureEcran", "Le fichier sélectionné est vide.");
                return;
            }

            if (fichier.ContentLength > TailleMaxCapture)
            {
                ModelState.AddModelError("CaptureEcran", "La capture ne doit pas dépasser 5 Mo.");
                return;
            }

            var extension = Path.GetExtension(fichier.FileName);
            var estPng = string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase);
            var estJpeg = string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase);
            var typeDeclare = (fichier.ContentType ?? string.Empty).ToLowerInvariant();

            if ((!estPng && !estJpeg)
                || (estPng && typeDeclare != "image/png")
                || (estJpeg && typeDeclare != "image/jpeg" && typeDeclare != "image/pjpeg"))
            {
                ModelState.AddModelError("CaptureEcran", "La capture doit être une image PNG ou JPEG valide.");
                return;
            }

            try
            {
                using (var image = Image.FromStream(fichier.InputStream, false, true))
                {
                    var formatAttendu = estPng ? ImageFormat.Png.Guid : ImageFormat.Jpeg.Guid;
                    if (image.RawFormat.Guid != formatAttendu)
                    {
                        ModelState.AddModelError("CaptureEcran", "Le contenu du fichier ne correspond pas à une image PNG ou JPEG valide.");
                    }
                }
            }
            catch (ArgumentException)
            {
                ModelState.AddModelError("CaptureEcran", "Le fichier sélectionné n’est pas une image PNG ou JPEG lisible.");
            }
            catch (OutOfMemoryException)
            {
                ModelState.AddModelError("CaptureEcran", "Le fichier sélectionné n’est pas une image PNG ou JPEG lisible.");
            }
        }
    }
}
