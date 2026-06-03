using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiTest.Data;
using WebApiTest.Models;

namespace WebApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiftMedController : ControllerBase
    {

        private readonly NOVOPHARMAContext _context2;
        private readonly MEDSOURCEContext _context;

        public GiftMedController(MEDSOURCEContext context, NOVOPHARMAContext context2)
        {
            _context = context;
            _context2 = context2;
        }

        [HttpPost("assign-gift")]
        public IActionResult AssignGiftToDermo([FromBody] GiftAssignment request)
        {
            if (request.QuantiteAttribuee <= 0)
            {
                return BadRequest(new { error = "La quantité attribuée doit être supérieure à 0." });
            }

            // Vérification si le GiftRef existe dans la base de données
            var exists = _context.MBAArticle.Any(a => a.ArRef == request.GiftRef);
            if (!exists)
            {
                return NotFound(new { error = "Le GiftRef fourni n'existe pas." });
            }

            // ✅ Insertion dans la base de données
            _context.GiftAssignments.Add(request);
            _context.SaveChanges();

            // ✅ Retourne une réponse JSON au lieu d'un texte brut
            return Ok(new { message = "Gift attribué avec succès." });
        }

        [HttpGet("gifts-by-dermo/{dermoId}")]
        public IActionResult GetGiftsByDermo(int dermoId)
        {
            var assignedGifts = (from g in _context.GiftAssignments
                                 join a in _context.MBAArticle on g.GiftRef equals a.ArRef
                                 where g.DermoId == dermoId
                                 select new
                                 {
                                     Id=g.Id,
                                     GiftRef = g.GiftRef,
                                     GiftName = a.ArDesign,      // Nom de l'article
                                     Marque = a.Marque,          // Marque du gift
                                     Photo = a.Ar_photo,         // Image du gift
                                     QuantiteAttribuee = g.QuantiteAttribuee,
                                     DateAttribution = g.DateAttribution
                                 }).ToList();

            if (!assignedGifts.Any())
            {
                return NotFound($"Aucun gift attribué au dermo-conseiller ID {dermoId}.");
            }

            return Ok(assignedGifts);
        }

        [HttpDelete("delete-assignment/{assignmentId}")]
        public IActionResult DeleteGiftAssignment(int assignmentId)
        {
            var assignment = _context.GiftAssignments.FirstOrDefault(g => g.Id == assignmentId);

            if (assignment == null)
            {
                return NotFound(new { error = "❌ Affectation introuvable." });
            }

            _context.GiftAssignments.Remove(assignment);
            _context.SaveChanges();

            return Ok(new { message = "✅ Affectation supprimée avec succès." });
        }

        [HttpPost("order-gift")]
        public IActionResult OrderGift([FromBody] OrderGiftRequest request)
        {
            try
            {
                Console.WriteLine("📥 Requête reçue pour une commande de gift");

                // Vérifier si le gift est attribué au dermo
                var giftAssignment = _context.GiftAssignments
                    .FirstOrDefault(g => g.GiftRef == request.GiftRef && g.DermoId == request.DermoId);

                if (giftAssignment == null)
                {
                    Console.WriteLine("❌ Aucun gift attribué !");
                    return NotFound(new { error = "Aucun gift attribué à ce dermo." });
                }

                // Vérifier la quantité commandée
                if (request.QuantiteCommande <= 0)
                {
                    Console.WriteLine("❌ Quantité commandée invalide !");
                    return BadRequest(new { error = "La quantité commandée doit être supérieure à 0." });
                }

                if (request.QuantiteCommande > giftAssignment.QuantiteAttribuee)
                {
                    Console.WriteLine($"❌ Stock insuffisant ! Disponible : {giftAssignment.QuantiteAttribuee}");
                    return BadRequest(new { error = $"Stock insuffisant ! Quantité dispo : {giftAssignment.QuantiteAttribuee}" });
                }

                // Déduire la quantité et enregistrer
                giftAssignment.QuantiteAttribuee -= request.QuantiteCommande;
                Console.WriteLine($"✅ Stock mis à jour : {giftAssignment.QuantiteAttribuee}");
                _context.SaveChanges();

                // 📌 Enregistrer la commande
                var order = new GiftOrder
                {
                    GiftRef = request.GiftRef,
                    DermoId = request.DermoId,
                    QuantiteCommande = request.QuantiteCommande,
                    CodePharmacie = request.CodePharmacie,
                    NomPharmacie = request.NomPharmacie,
                    DateCommande = DateTime.UtcNow
                };

                _context.GiftOrders.Add(order);
                _context.SaveChanges();
                Console.WriteLine("✅ Commande enregistrée en base !");

                return Ok(new
                {
                    message = "✅ Commande enregistrée avec succès.",
                    GiftRef = order.GiftRef,
                    DermoId = order.DermoId,
                    QuantiteCommande = order.QuantiteCommande,
                    CodePharmacie = order.CodePharmacie,
                    NomPharmacie = order.NomPharmacie,
                    DateCommande = order.DateCommande
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur interne : {ex.Message}");
                return StatusCode(500, new { error = "Erreur interne, veuillez réessayer plus tard." });
            }
        }

        [HttpGet("gift-orders")]
        public IActionResult GetAllGiftOrders()
        {
            var giftOrders = (from o in _context.GiftOrders
                              join a in _context.MBAArticle on o.GiftRef equals a.ArRef
                              join d in _context2.Authentication on o.DermoId equals d.id
                              select new
                              {
                                  // Infos du gift
                                  GiftRef = o.GiftRef,
                                  GiftName = a.ArDesign,      // Nom du gift
                                  Marque = a.Marque,          // Marque
              
                                  Photo = a.Ar_photo,         // Image

                                  // Infos de la commande
                                  QuantiteCommande = o.QuantiteCommande,
                                  DateCommande = o.DateCommande,
                                  
                                  // Infos du dermo-conseiller
                                  DermoId = d.id,
                                  DermoName = d.username,

                                  // Infos de la pharmacie
                                  CodePharmacie = o.CodePharmacie,
                                  NomPharmacie = o.NomPharmacie
                              }).ToList();

            if (!giftOrders.Any())
            {
                return NotFound("Aucune commande de gift trouvée.");
            }

            return Ok(giftOrders);
        }
        [HttpGet("gift-orders-by-dermo/{dermoId}")]
        public IActionResult GetGiftOrdersByDermo(int dermoId)
        {
            var giftOrders = (from o in _context.GiftOrders
                              join a in _context.MBAArticle on o.GiftRef equals a.ArRef
                              join d in _context2.Authentication on o.DermoId equals d.id
                              where o.DermoId == dermoId
                              select new
                              {
                                  // Infos du gift
                                  GiftRef = o.GiftRef,
                                  GiftName = a.ArDesign,
                                  Marque = a.Marque,
                                  PrixAchat = a.ArPrixAch,
                                  PrixVente = a.ArPrixVen,
                                  Photo = a.Ar_photo,

                                  // Infos de la commande
                                  QuantiteCommande = o.QuantiteCommande,
                                  DateCommande = o.DateCommande,

                                  // Infos du dermo-conseiller
                                  DermoId = d.id,
                                  DermoName = d.username,

                                  // Infos de la pharmacie associée
                                  CodePharmacie = o.CodePharmacie,
                                  NomPharmacie = o.NomPharmacie
                              }).ToList();

            if (!giftOrders.Any())
            {
                return NotFound($"Aucune commande trouvée pour le dermo-conseiller ID {dermoId}.");
            }

            return Ok(giftOrders);
        }
        [HttpGet("gift-orders-by-pharmacie/{codePharmacie}")]
        public IActionResult GetGiftOrdersByPharmacie(string codePharmacie)
        {
            var giftOrders = (from o in _context.GiftOrders
                              join a in _context.MBAArticle on o.GiftRef equals a.ArRef
                              join d in _context2.Authentication on o.DermoId equals d.id
                              where o.CodePharmacie == codePharmacie
                              select new
                              {
                                  // Infos du gift
                                  GiftRef = o.GiftRef,
                                  GiftName = a.ArDesign,
                                  Marque = a.Marque,
                                  PrixAchat = a.ArPrixAch,
                                  PrixVente = a.ArPrixVen,
                                  Photo = a.Ar_photo,
                        
                                  // Infos de la commande
                                  QuantiteCommande = o.QuantiteCommande,
                                  DateCommande = o.DateCommande,

                                  // Infos du dermo-conseiller
                                  DermoId = d.id,
                                  DermoName = d.username,

                                  // Infos de la pharmacie associée
                                  CodePharmacie = o.CodePharmacie,
                                  NomPharmacie = o.NomPharmacie
                              }).ToList();

            if (!giftOrders.Any())
            {
                return NotFound($"Aucune commande trouvée pour la pharmacie {codePharmacie}.");
            }

            return Ok(giftOrders);
        }


    }
}
