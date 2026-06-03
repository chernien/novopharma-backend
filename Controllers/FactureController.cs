using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using WebApiTest.Data;
using WebApiTest.Models;

namespace WebApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FactureController : ControllerBase
    {
        private readonly NOVOPHARMAContext _context;

        public FactureController(NOVOPHARMAContext context)
        {
            _context = context;
        }

        // 1. Récupérer toutes les factures
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Facture>>> GetAllFactures()
        {
            return await _context.Factures.ToListAsync();
        }

        // 2. Récupérer une facture par son ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Facture>> GetFactureById(int id)
        {
            var facture = await _context.Factures.FindAsync(id);

            if (facture == null)
            {
                return NotFound("Facture introuvable !");
            }

            return Ok(facture);
        }

        //// 3. Ajouter une nouvelle facture
        //[HttpPost]
        //public async Task<IActionResult> AddFacture([FromForm] FactureViewModel formData)
        //{
        //    try
        //    {
        //        Console.WriteLine("📥 Requête reçue");

        //        string fileUrl = null;

        //        // 🔹 Vérifier si un fichier est envoyé
        //        if (formData.File != null && formData.File.Length > 0)
        //        {
        //            Console.WriteLine($"✅ Fichier reçu : {formData.File.FileName} ({formData.File.Length} octets)");

        //            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "factures");

        //            if (!Directory.Exists(uploadsFolder))
        //            {
        //                Directory.CreateDirectory(uploadsFolder);
        //            }

        //            string uniqueFileName = $"{formData.Nom}_{Guid.NewGuid()}{Path.GetExtension(formData.File.FileName)}";
        //            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await formData.File.CopyToAsync(stream);
        //            }

        //            Console.WriteLine($"✅ Image enregistrée à : {filePath}");

        //            // 🔹 Générer l'URL du fichier
        //            fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/factures/{uniqueFileName}";
        //        }
        //        else
        //        {
        //            Console.WriteLine("⚠️ Aucun fichier reçu, la facture sera ajoutée sans image.");
        //        }

        //        // 🔹 Création de la facture
        //        var facture = new Facture
        //        {
        //            Nom = formData.Nom,
        //            Path = fileUrl, // ✅ Peut être NULL si aucun fichier
        //            CodePharmacie = formData.CodePharmacie,
        //            NomPharmacie = formData.NomPharmacie,
        //            NomDermo = formData.NomDermo,
        //            // 🔥 Convertir la date reçue (string) → DateTime avec l'heure
        //            DateFacture = formData.DateFacture, 
        //        DateArrivee = formData.DateArrivee,
        //            DateSortie = formData.DateSortie,
        //            Commentaire = formData.Commentaire
        //        };

        //        _context.Factures.Add(facture);
        //        int rows = await _context.SaveChangesAsync();

        //        if (rows > 0)
        //        {
        //            Console.WriteLine("✅ Facture ajoutée en base !");
        //            return Ok(new { message = "✅ Facture ajoutée avec succès.", fileUrl });
        //        }
        //        else
        //        {
        //            Console.WriteLine("❌ Échec de l'ajout en base !");
        //            return StatusCode(500, "❌ Facture non enregistrée en base.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("❌ Erreur backend : " + ex.Message);
        //        return StatusCode(500, $"❌ Erreur interne : {ex.Message}");
        //    }
        //}


        [HttpPost]
        public async Task<IActionResult> AddFacture([FromForm] FactureViewModel formData)
        {
            try
            {
                Console.WriteLine("📥 Requête reçue");

                string fileUrl = null;

                if (formData.File != null && formData.File.Length > 0)
                {
                    Console.WriteLine($"✅ Fichier reçu : {formData.File.FileName}");

                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "factures");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = $"{formData.Nom}_{Guid.NewGuid()}{Path.GetExtension(formData.File.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await formData.File.CopyToAsync(stream);

                    fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/factures/{uniqueFileName}";
                }

                var facture = new Facture
                {
                    Nom = formData.Nom,
                    Path = fileUrl,
                    CodePharmacie = formData.CodePharmacie,
                    NomPharmacie = formData.NomPharmacie,
                    NomDermo = formData.NomDermo,
                    DateFacture = formData.DateFacture,
                    DateArrivee = formData.DateArrivee,
                    DateSortie = formData.DateSortie,
                    Commentaire = formData.Commentaire
                };

                _context.Factures.Add(facture);
                int rows = await _context.SaveChangesAsync();

                if (rows > 0)
                    return Ok(new { message = "✅ Facture ajoutée avec succès.", fileUrl });

                // SaveChanges a réussi mais 0 lignes → cas rare
                return StatusCode(500, new
                {
                    errorType = "DATABASE_SAVE_ERROR",
                    message = "❌ La facture n'a pas pu être enregistrée en base de données."
                });
            }
            catch (DbUpdateException dbEx)
            {
                var detail = dbEx.InnerException?.Message ?? dbEx.Message;
                Console.WriteLine("❌ DbUpdateException : " + detail);
                return StatusCode(500, new
                {
                    errorType = "DATABASE_SAVE_ERROR",
                    message = "❌ Erreur lors de l'enregistrement de la facture en base de données.",
                    detail
                });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                var innerType = ex.InnerException?.GetType().FullName ?? ex.GetType().FullName ?? "";

                bool isSqlConnection =
                    innerType.Contains("SqlException") ||
                    innerMessage.Contains("SQL Server") ||
                    innerMessage.Contains("Cannot open database") ||
                    innerMessage.Contains("A network-related");

                Console.WriteLine("❌ Erreur backend : " + innerMessage);

                if (isSqlConnection)
                    return StatusCode(500, new
                    {
                        errorType = "DATABASE_CONNECTION_ERROR",
                        message = "❌ Impossible de se connecter à la base de données. Vérifiez que SQL Server est démarré.",
                        detail = innerMessage
                    });

                return StatusCode(500, new
                {
                    errorType = "SERVER_ERROR",
                    message = "❌ Erreur interne du serveur.",
                    detail = ex.Message
                });
            }
        }

        // 4. Mettre à jour une facture existante
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFacture(int id, [FromForm] FactureViewModel formData)
        {
            try
            {
                var facture = await _context.Factures.FindAsync(id);

                if (facture == null)
                {
                    return NotFound("Facture introuvable !");
                }

                // Gérer le fichier si présent
                if (formData.File != null && formData.File.Length > 0)
                {
                    string uploadsFolder = Path.Combine("C:\\Uploads\\Factures");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = $"{id}_{formData.File.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formData.File.CopyToAsync(stream);
                    }

                    facture.Path = filePath;
                }

                // Mettre à jour les autres champs
                facture.Nom = formData.Nom;
                facture.CodePharmacie = formData.CodePharmacie;
                facture.NomPharmacie = formData.NomPharmacie;
                facture.NomDermo = formData.NomDermo;
                facture.DateFacture = formData.DateFacture;
                facture.Commentaire = formData.Commentaire;

                await _context.SaveChangesAsync();

                return Ok(facture);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Une erreur s'est produite : {ex.Message}");
                return StatusCode(500, "Une erreur interne s'est produite.");
            }
        }
        [HttpGet("facture/image/{id}")]
        public IActionResult GetFactureImage(int id)
        {
            // Recherche de la facture dans la base de données
            var facture = _context.Factures.Find(id);

            if (facture == null || string.IsNullOrEmpty(facture.Path))
            {
                return NotFound("Facture introuvable ou aucune image associée.");
            }

            // Chemin complet vers l'image
            string imagePath = facture.Path;

            // Vérifiez si le fichier existe
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound("Le fichier image n'existe pas sur le serveur.");
            }

            try
            {
                // Lire les données de l'image
                byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);

                // Déterminer le type de contenu de l'image
                string contentType = GetContentType(Path.GetExtension(imagePath));

                // Retourner l'image comme réponse HTTP avec le type de contenu approprié
                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                // Gérer les erreurs (ex : problème d'accès au fichier)
                Console.WriteLine($"Erreur lors de la lecture de l'image : {ex.Message}");
                return StatusCode(500, "Erreur interne lors de la récupération de l'image.");
            }
        }

        // Méthode pour déterminer le type de contenu basé sur l'extension de fichier
        private string GetContentType(string fileExtension)
        {
            fileExtension = fileExtension.ToLowerInvariant();

            return fileExtension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".tiff" => "image/tiff",
                _ => "application/octet-stream",
            };
        }

    }


    // ViewModel pour gérer les données reçues avec un fichier
    public class FactureViewModel
    {
        public string Nom { get; set; }
        public string CodePharmacie { get; set; }
        public string NomPharmacie { get; set; }
        public string NomDermo { get; set; }
        public DateTime? DateFacture { get; set; }
        public string? Commentaire { get; set; }
        public DateTime? DateArrivee { get; set; }
        public DateTime? DateSortie { get; set; }
        public IFormFile? File { get; set; }

    }
}

