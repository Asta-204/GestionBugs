using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GestionBugs.Models
{
    public class BugSearchViewModel
    {
        [StringLength(4000, ErrorMessage = "La recherche ne peut pas dépasser 4 000 caractères.")]
        [Display(Name = "Rechercher un bug")]
        public string Recherche { get; set; }

        public IList<BugSearchResultViewModel> Resultats { get; set; }
    }

    public class BugSearchResultViewModel
    {
        public int IdBug { get; set; }
        public string Titre { get; set; }
        public string Application { get; set; }
        public string Categorie { get; set; }
        public string Description { get; set; }
        public string Priorite { get; set; }
        public DateTime DateCreationUtc { get; set; }
    }
}
