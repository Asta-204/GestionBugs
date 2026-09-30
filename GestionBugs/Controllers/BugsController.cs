using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using GestionBugs.Data;
using GestionBugs.Models;

namespace GestionBugs.Controllers
{
    public class BugsController : Controller
    {
        private const int TailleMaxCapture = 5 * 1024 * 1024;
        private static readonly string[] Priorites = { "Faible", "Normale", "Haute", "Critique" };

        [HttpGet]
        public ActionResult Index()
        {
            return AfficherRecherche(new BugSearchViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(BugSearchViewModel modele)
        {
            return AfficherRecherche(modele ?? new BugSearchViewModel());
        }

        private ActionResult AfficherRecherche(BugSearchViewModel modele)
        {
            if (!ModelState.IsValid)
            {
                modele.Resultats = new System.Collections.Generic.List<BugSearchResultViewModel>();
                return View(modele);
            }

            try
            {
                modele.Resultats = new BugRepository().Rechercher(modele.Recherche);
                return View(modele);
            }
            catch (SqlException)
            {
                return ErreurLecture();
            }
        }

        [HttpGet]
        public ActionResult Creer()
        {
            return View(new BugDeclarationViewModel { Priorite = "Normale" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Creer(BugDeclarationViewModel modele)
        {
            if (modele == null)
            {
                modele = new BugDeclarationViewModel();
            }

            if (!Priorites.Contains(modele.Priorite))
            {
                ModelState.AddModelError("Priorite", "Veuillez choisir une priorité valide.");
            }
            if (!BugCategories.Valeurs.Contains(modele.Categorie))
            {
                ModelState.AddModelError("Categorie", "Veuillez choisir une catégorie valide.");
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

            byte[] contenuCapture = null;
            string mimeCapture = null;
            string nomCapture = null;
            if (captureSelectionnee)
            {
                nomCapture = Path.GetFileName(fichier.FileName);
                if (nomCapture.Length > 255)
                {
                    ModelState.AddModelError("CaptureEcran", "Le nom de la capture ne peut pas dépasser 255 caractères.");
                    return View(modele);
                }

                contenuCapture = LireCapture(fichier);
                mimeCapture = Path.GetExtension(nomCapture).Equals(".png", StringComparison.OrdinalIgnoreCase)
                    ? "image/png"
                    : "image/jpeg";
            }

            try
            {
                var idBug = new BugRepository().Creer(modele, contenuCapture, mimeCapture, nomCapture);
                TempData["Confirmation"] = "Le bug a été enregistré avec succès.";
                return RedirectToAction("Details", new { id = idBug });
            }
            catch (SqlException)
            {
                ModelState.AddModelError(string.Empty, "L’enregistrement a échoué. Vérifiez la connexion SQL Server et la disponibilité des procédures.");
                return View(modele);
            }
        }

        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return HttpNotFound();
            }

            try
            {
                var depot = new BugRepository();
                var modele = depot.ChargerBug(id.Value);
                if (modele == null)
                {
                    return HttpNotFound();
                }

                modele.CaptureDisponible = depot.ChargerCapture(id.Value) != null;
                ViewBag.Confirmation = TempData["Confirmation"];
                return View(modele);
            }
            catch (SqlException)
            {
                return ErreurLecture();
            }
        }

        [HttpGet]
        public ActionResult Capture(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                return HttpNotFound();
            }

            try
            {
                var capture = new BugRepository().ChargerCapture(id.Value);
                if (capture == null)
                {
                    return HttpNotFound();
                }

                return File(capture.Contenu, capture.MimeType);
            }
            catch (SqlException)
            {
                return ErreurLecture();
            }
        }

        private ActionResult ErreurLecture()
        {
            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;
            return View("Erreur", "Impossible de charger les informations du bug. Vérifiez la connexion SQL Server et les procédures.");
        }

        private static byte[] LireCapture(System.Web.HttpPostedFileBase fichier)
        {
            var flux = fichier.InputStream;
            if (flux.CanSeek)
            {
                flux.Position = 0;
            }

            using (var contenu = new MemoryStream())
            {
                flux.CopyTo(contenu);
                return contenu.ToArray();
            }
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
            finally
            {
                if (fichier.InputStream.CanSeek)
                {
                    fichier.InputStream.Position = 0;
                }
            }
        }
    }
}
