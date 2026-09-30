using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using GestionBugs.Models;

namespace GestionBugs.Data
{
    public class BugRepository
    {
        private readonly string _connectionString;

        public BugRepository()
        {
            var configuration = ConfigurationManager.ConnectionStrings["GestionBugs"];
            if (configuration == null || string.IsNullOrWhiteSpace(configuration.ConnectionString))
            {
                throw new ConfigurationErrorsException("La connexion SQL Server GestionBugs n’est pas configurée.");
            }

            _connectionString = configuration.ConnectionString;
        }

        public int Creer(BugDeclarationViewModel modele, byte[] capture, string mimeCapture, string nomCapture)
        {
            using (var connexion = new SqlConnection(_connectionString))
            using (var commande = new SqlCommand("dbo.GB_P_CREER_BUG", connexion))
            {
                commande.CommandType = CommandType.StoredProcedure;
                commande.Parameters.Add(new SqlParameter("@Titre", SqlDbType.NVarChar, 150) { Value = modele.Titre });
                AjouterTexteLong(commande, "@Application", modele.Application);
                commande.Parameters.Add(new SqlParameter("@Categorie", SqlDbType.NVarChar, 30) { Value = modele.Categorie });
                AjouterTexteLong(commande, "@Description", modele.Description);
                AjouterTexteLong(commande, "@Etapes", modele.Etapes);
                AjouterTexteLong(commande, "@ResultatAttendu", modele.ResultatAttendu);
                AjouterTexteLong(commande, "@ResultatObtenu", modele.ResultatObtenu);
                AjouterTexteLong(commande, "@MessageErreur", modele.MessageErreur);
                commande.Parameters.Add(new SqlParameter("@Priorite", SqlDbType.NVarChar, 10) { Value = modele.Priorite });

                var identifiant = new SqlParameter("@IdBug", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                commande.Parameters.Add(identifiant);

                var parametreCapture = commande.Parameters.Add("@CaptureContenu", SqlDbType.VarBinary, -1);
                parametreCapture.Value = capture == null ? (object)DBNull.Value : capture;
                var parametreMime = commande.Parameters.Add("@CaptureMime", SqlDbType.VarChar, 20);
                parametreMime.Value = mimeCapture == null ? (object)DBNull.Value : mimeCapture;
                var parametreNom = commande.Parameters.Add("@CaptureNomFichier", SqlDbType.NVarChar, 255);
                parametreNom.Value = nomCapture == null ? (object)DBNull.Value : nomCapture;

                connexion.Open();
                commande.ExecuteNonQuery();
                return Convert.ToInt32(identifiant.Value);
            }
        }

        public IList<BugSearchResultViewModel> Rechercher(string recherche)
        {
            const string requete = @"
SELECT IdBug, Titre, Application, Categorie, Description, Priorite, DateCreationUtc
FROM dbo.GB_BUG
WHERE @Recherche = N''
   OR CHARINDEX(@Recherche, Titre) > 0
   OR CHARINDEX(@Recherche, Application) > 0
   OR CHARINDEX(@Recherche, Categorie) > 0
   OR CHARINDEX(@Recherche, Description) > 0
   OR CHARINDEX(@Recherche, Etapes) > 0
   OR CHARINDEX(@Recherche, ResultatAttendu) > 0
   OR CHARINDEX(@Recherche, ResultatObtenu) > 0
   OR CHARINDEX(@Recherche, MessageErreur) > 0
   OR CHARINDEX(@Recherche, Priorite) > 0
ORDER BY DateCreationUtc DESC, IdBug DESC;";

            var resultats = new List<BugSearchResultViewModel>();
            using (var connexion = new SqlConnection(_connectionString))
            using (var commande = new SqlCommand(requete, connexion))
            {
                commande.CommandType = CommandType.Text;
                commande.Parameters.Add(new SqlParameter("@Recherche", SqlDbType.NVarChar, 4000)
                {
                    Value = (object)(recherche ?? string.Empty).Trim()
                });
                connexion.Open();

                using (var lecteur = commande.ExecuteReader())
                {
                    while (lecteur.Read())
                    {
                        resultats.Add(new BugSearchResultViewModel
                        {
                            IdBug = lecteur.GetInt32(lecteur.GetOrdinal("IdBug")),
                            Titre = LireTexte(lecteur, "Titre"),
                            Application = LireTexte(lecteur, "Application"),
                            Categorie = LireTexte(lecteur, "Categorie"),
                            Description = LireTexte(lecteur, "Description"),
                            Priorite = LireTexte(lecteur, "Priorite"),
                            DateCreationUtc = lecteur.GetDateTime(lecteur.GetOrdinal("DateCreationUtc"))
                        });
                    }
                }
            }

            return resultats;
        }

        public BugDetailViewModel ChargerBug(int idBug)
        {
            using (var connexion = new SqlConnection(_connectionString))
            using (var commande = new SqlCommand("dbo.GB_P_CHARGER_BUG", connexion))
            {
                commande.CommandType = CommandType.StoredProcedure;
                commande.Parameters.Add(new SqlParameter("@IdBug", SqlDbType.Int) { Value = idBug });
                connexion.Open();

                using (var lecteur = commande.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!lecteur.Read())
                    {
                        return null;
                    }

                    return new BugDetailViewModel
                    {
                        IdBug = lecteur.GetInt32(lecteur.GetOrdinal("IdBug")),
                        Titre = LireTexte(lecteur, "Titre"),
                        Application = LireTexte(lecteur, "Application"),
                        Categorie = LireTexte(lecteur, "Categorie"),
                        Description = LireTexte(lecteur, "Description"),
                        Etapes = LireTexte(lecteur, "Etapes"),
                        ResultatAttendu = LireTexte(lecteur, "ResultatAttendu"),
                        ResultatObtenu = LireTexte(lecteur, "ResultatObtenu"),
                        MessageErreur = LireTexte(lecteur, "MessageErreur"),
                        Priorite = LireTexte(lecteur, "Priorite"),
                        DateCreationUtc = lecteur.GetDateTime(lecteur.GetOrdinal("DateCreationUtc"))
                    };
                }
            }
        }

        public BugCapture ChargerCapture(int idBug)
        {
            using (var connexion = new SqlConnection(_connectionString))
            using (var commande = new SqlCommand("dbo.GB_P_CHARGER_CAPTURE", connexion))
            {
                commande.CommandType = CommandType.StoredProcedure;
                commande.Parameters.Add(new SqlParameter("@IdBug", SqlDbType.Int) { Value = idBug });
                connexion.Open();

                using (var lecteur = commande.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!lecteur.Read())
                    {
                        return null;
                    }

                    return new BugCapture
                    {
                        MimeType = lecteur.GetString(lecteur.GetOrdinal("MimeType")),
                        NomFichier = lecteur.GetString(lecteur.GetOrdinal("NomFichier")),
                        Contenu = (byte[])lecteur["Contenu"]
                    };
                }
            }
        }

        private static void AjouterTexteLong(SqlCommand commande, string nom, string valeur)
        {
            var parametre = commande.Parameters.Add(nom, SqlDbType.NVarChar, -1);
            parametre.Value = string.IsNullOrWhiteSpace(valeur) ? (object)DBNull.Value : valeur;
        }

        private static string LireTexte(SqlDataReader lecteur, string nom)
        {
            var ordinal = lecteur.GetOrdinal(nom);
            return lecteur.IsDBNull(ordinal) ? null : lecteur.GetString(ordinal);
        }
    }
}
