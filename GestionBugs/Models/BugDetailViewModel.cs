using System;

namespace GestionBugs.Models
{
    public class BugDetailViewModel
    {
        public int IdBug { get; set; }
        public string Titre { get; set; }
        public string Application { get; set; }
        public string Categorie { get; set; }
        public string Description { get; set; }
        public string Etapes { get; set; }
        public string ResultatAttendu { get; set; }
        public string ResultatObtenu { get; set; }
        public string MessageErreur { get; set; }
        public string Priorite { get; set; }
        public DateTime DateCreationUtc { get; set; }
        public bool CaptureDisponible { get; set; }
    }

    public class BugCapture
    {
        public byte[] Contenu { get; set; }
        public string MimeType { get; set; }
        public string NomFichier { get; set; }
    }
}
