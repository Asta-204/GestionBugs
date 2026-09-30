using System.ComponentModel.DataAnnotations;
using System.Web;

namespace GestionBugs.Models
{
    public class BugDeclarationViewModel
    {
        [Required(ErrorMessage = "Le titre est obligatoire.")]
        [StringLength(150, ErrorMessage = "Le titre ne peut pas dépasser 150 caractères.")]
        [Display(Name = "Titre")]
        public string Titre { get; set; }

        [Required(ErrorMessage = "L’application ou le module concerné est obligatoire.")]
        [Display(Name = "Application ou module concerné")]
        public string Application { get; set; }

        [Required(ErrorMessage = "La description détaillée est obligatoire.")]
        [Display(Name = "Description détaillée")]
        public string Description { get; set; }

        [Display(Name = "Étapes pour reproduire le bug")]
        public string Etapes { get; set; }

        [Display(Name = "Résultat attendu")]
        public string ResultatAttendu { get; set; }

        [Display(Name = "Résultat obtenu")]
        public string ResultatObtenu { get; set; }

        [Display(Name = "Message d’erreur")]
        public string MessageErreur { get; set; }

        [Required(ErrorMessage = "Veuillez choisir une priorité.")]
        [Display(Name = "Priorité")]
        public string Priorite { get; set; }

        [Display(Name = "Capture d’écran (facultative)")]
        public HttpPostedFileBase CaptureEcran { get; set; }
    }
}
