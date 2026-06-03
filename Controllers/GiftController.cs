using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiTest.Data;
using WebApiTest.Models;

namespace WebApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GiftController : ControllerBase
    {

        private readonly NOVOPHARMAContext _context;
        private readonly MEDSOURCEContext _context2;

        public GiftController(NOVOPHARMAContext context, MEDSOURCEContext context2)
        {
            _context = context;
            _context2 = context2;
        }
        [HttpGet("all-gift")]
        [ProducesResponseType(200, Type = typeof(ICollection<object>))]
        public IActionResult GetAllArticlesGifts()
        {
            // 🔹 Familles pour Medsource
            var famillesMedsource = new List<string>
    {
        "0509", "0508", "0507", "0502", "0501", "05014"
    };

            // 🔹 Référence spéciale Medsource
            var specialRefsMedsource = new List<string> { "000000144" };

            // 🔹 Articles Medsource
            var articlesMedsource = _context2.MBAArticle
                .AsNoTracking()
                .Where(m =>
                    (famillesMedsource.Contains(m.FaCodeFamille) && m.ArRef != "050140022")
                    || specialRefsMedsource.Contains(m.ArRef)   // 🔥 AJOUT ICI
                )
                .Select(a => new
                {
                    a.ArRef,
                    a.ArDesign,
                    a.Marque,
                    a.FaCodeFamille,
                    a.Ar_photo,
                    a.Famille,
                    a.AR_Sommeil,
                    a.Recommande,
                    Source = "Medsource"
                })
                .ToList();

            // 🔹 Articles Novopharma
            var specialRefsNovopharma = new List<string>
    {
        "010070028", "050050369"
    };

            var articlesNovopharma = _context.MBAArticle
                .AsNoTracking()
                .Where(m =>
                       m.FaCodeFamille.StartsWith("0501") ||
                       m.FaCodeFamille.StartsWith("0400") ||
                       m.FaCodeFamille.StartsWith("05004") ||
                       specialRefsNovopharma.Contains(m.ArRef)
                )
                .Select(a => new
                {
                    a.ArRef,
                    a.ArDesign,
                    a.Marque,
                    a.FaCodeFamille,
                    a.Ar_photo,
                    a.Famille,
                    a.AR_Sommeil,
                    a.Recommande,
                    Source = "Novopharma"
                })
                .ToList();

            // 🔹 Fusion
            var allArticlesGifts = articlesNovopharma
                .Concat(articlesMedsource)
                .ToList();

            return Ok(allArticlesGifts);
        }







        [HttpPost("assign-gift")]
        public IActionResult AssignGiftToDermo([FromBody] GiftAssignment request)
        {
            try
            {
                if (request.QuantiteAttribuee <= 0)
                {
                    return BadRequest(new { error = "La quantité attribuée doit être supérieure à 0." });
                }

                // =========================
                // 🔎 Vérification GiftRef dans les deux bases
                // =========================
                var existsInNovopharma = _context.MBAArticle.Any(a => a.ArRef == request.GiftRef);
                var existsInMedsource = _context2.MBAArticle.Any(a => a.ArRef == request.GiftRef);

                if (!existsInNovopharma && !existsInMedsource)
                {
                    return NotFound(new { error = "Le GiftRef fourni n'existe dans aucune base." });
                }

                object resultNovopharma = null;
                object resultMedsource = null;

                // ==================================================
                // 🟦 TRAITEMENT NOVOPHARMA (_context)
                // ==================================================
                if (existsInNovopharma)
                {
                    var existingAssignment = _context.GiftAssignments
                        .FirstOrDefault(g => g.GiftRef == request.GiftRef && g.DermoId == request.DermoId);

                    if (existingAssignment != null)
                    {
                        existingAssignment.QuantiteAttribuee += request.QuantiteAttribuee;
                        existingAssignment.DateAttribution = DateTime.UtcNow;

                        _context.SaveChanges();

                        resultNovopharma = new
                        {
                            message = "Gift mis à jour avec succès (Novopharma).",
                            assignment = existingAssignment
                        };
                    }
                    else
                    {
                        var newAssignment = new GiftAssignment
                        {
                            GiftRef = request.GiftRef,
                            DermoId = request.DermoId,
                            QuantiteAttribuee = request.QuantiteAttribuee,
                            DateAttribution = DateTime.UtcNow
                        };

                        _context.GiftAssignments.Add(newAssignment);
                        _context.SaveChanges();

                        resultNovopharma = new
                        {
                            message = "Gift attribué avec succès (Novopharma).",
                            assignment = newAssignment
                        };
                    }
                }

                // ==================================================
                // 🟩 TRAITEMENT MEDSOURCE (_context2)
                // ==================================================
                if (existsInMedsource)
                {
                    var existingAssignment = _context2.GiftAssignments
                        .FirstOrDefault(g => g.GiftRef == request.GiftRef && g.DermoId == request.DermoId);

                    if (existingAssignment != null)
                    {
                        existingAssignment.QuantiteAttribuee += request.QuantiteAttribuee;
                        existingAssignment.DateAttribution = DateTime.UtcNow;

                        _context2.SaveChanges();

                        resultMedsource = new
                        {
                            message = "Gift mis à jour avec succès (Medsource).",
                            assignment = existingAssignment
                        };
                    }
                    else
                    {
                        var newAssignment = new GiftAssignment
                        {
                            GiftRef = request.GiftRef,
                            DermoId = request.DermoId,
                            QuantiteAttribuee = request.QuantiteAttribuee,
                            DateAttribution = DateTime.UtcNow
                        };

                        _context2.GiftAssignments.Add(newAssignment);
                        _context2.SaveChanges();

                        resultMedsource = new
                        {
                            message = "Gift attribué avec succès (Medsource).",
                            assignment = newAssignment
                        };
                    }
                }

                // =========================
                // ✅ RÉPONSE FINALE
                // =========================
                return Ok(new
                {
                    message = "Attribution du gift traitée avec succès.",
                    novopharma = resultNovopharma,
                    medsource = resultMedsource
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Erreur interne, veuillez réessayer plus tard.",
                    details = ex.Message
                });
            }
        }


        [HttpGet("gifts-by-dermo/{dermoId}")]
        public IActionResult GetGiftsByDermo(int dermoId)
        {
            // 🧹 Nettoyage Novopharma
            var zeroNovopharma = _context.GiftAssignments
                .Where(g => g.DermoId == dermoId && g.QuantiteAttribuee == 0)
                .ToList();

            if (zeroNovopharma.Any())
            {
                _context.GiftAssignments.RemoveRange(zeroNovopharma);
                _context.SaveChanges();
            }

            // 🧹 Nettoyage Medsource
            var zeroMedsource = _context2.GiftAssignments
                .Where(g => g.DermoId == dermoId && g.QuantiteAttribuee == 0)
                .ToList();

            if (zeroMedsource.Any())
            {
                _context2.GiftAssignments.RemoveRange(zeroMedsource);
                _context2.SaveChanges();
            }

            // ✅ TON CODE ACTUEL (inchangé)
            var novopharma = (
                from g in _context.GiftAssignments
                join a in _context.MBAArticle on g.GiftRef equals a.ArRef
                where g.DermoId == dermoId && g.QuantiteAttribuee > 0
                select new
                {
                    g.Id,
                    g.GiftRef,
                    GiftName = a.ArDesign,
                    a.Marque,
                    Photo = a.Ar_photo,
                    g.QuantiteAttribuee,
                    g.DateAttribution,
                    Source = "Novopharma"
                }
            )
            .AsEnumerable()
            .GroupBy(x => new { x.GiftRef, x.Source })
            .Select(g =>
            {
                var last = g.OrderByDescending(x => x.DateAttribution).First();
                return new
                {
                    Id = last.Id,
                    GiftRef = last.GiftRef,
                    GiftName = last.GiftName,
                    Marque = last.Marque,
                    Photo = last.Photo,
                    QuantiteAttribuee = g.Sum(x => x.QuantiteAttribuee),
                    DateAttribution = last.DateAttribution,
                    Source = last.Source
                };
            })
            .ToList();

            var medsource = (
                from g in _context2.GiftAssignments
                join a in _context2.MBAArticle on g.GiftRef equals a.ArRef
                where g.DermoId == dermoId && g.QuantiteAttribuee > 0
                select new
                {
                    g.Id,
                    g.GiftRef,
                    GiftName = a.ArDesign,
                    a.Marque,
                    Photo = a.Ar_photo,
                    g.QuantiteAttribuee,
                    g.DateAttribution,
                    Source = "Medsource"
                }
            )
            .AsEnumerable()
            .GroupBy(x => new { x.GiftRef, x.Source })
            .Select(g =>
            {
                var last = g.OrderByDescending(x => x.DateAttribution).First();
                return new
                {
                    Id = last.Id,
                    GiftRef = last.GiftRef,
                    GiftName = last.GiftName,
                    Marque = last.Marque,
                    Photo = last.Photo,
                    QuantiteAttribuee = g.Sum(x => x.QuantiteAttribuee),
                    DateAttribution = last.DateAttribution,
                    Source = last.Source
                };
            })
            .ToList();

            var allAssignedGifts = novopharma
                .Concat(medsource)
                .OrderByDescending(x => x.DateAttribution)
                .ToList();

            if (!allAssignedGifts.Any())
            {
                return NotFound($"Aucun gift attribué au dermo-conseiller ID {dermoId}.");
            }

            return Ok(allAssignedGifts);
        }




        [HttpDelete("delete-assignment/{assignmentId}")]
        public IActionResult DeleteGiftAssignment(int assignmentId)
        {
            // 🔹 Cherche d'abord dans context1
            var assignment = _context.GiftAssignments.FirstOrDefault(g => g.Id == assignmentId);

            // 🔹 Si introuvable dans context1, cherche dans context2
            if (assignment == null)
            {
                assignment = _context2.GiftAssignments.FirstOrDefault(g => g.Id == assignmentId);

                if (assignment == null)
                {
                    return NotFound(new { error = "❌ Affectation introuvable dans les deux contextes." });
                }

                // Supprime depuis context2
                _context2.GiftAssignments.Remove(assignment);
                _context2.SaveChanges();

                return Ok(new { message = "✅ Affectation supprimée avec succès dans context2." });
            }

            // Supprime depuis context1
            _context.GiftAssignments.Remove(assignment);
            _context.SaveChanges();

            return Ok(new { message = "✅ Affectation supprimée avec succès dans context1." });
        }
        [HttpPost("order-gift")]
        public IActionResult OrderGift([FromBody] OrderGiftRequest request)
        {
            try
            {
                Console.WriteLine("📥 Requête reçue pour une commande de gift");

                // 🔹 Récupération attribution gift
                var giftAssignment1 = _context.GiftAssignments
                    .FirstOrDefault(g => g.GiftRef == request.GiftRef && g.DermoId == request.DermoId);

                var giftAssignment2 = giftAssignment1 == null
                    ? _context2.GiftAssignments
                        .FirstOrDefault(g => g.GiftRef == request.GiftRef && g.DermoId == request.DermoId)
                    : null;

                if (giftAssignment1 == null && giftAssignment2 == null)
                {
                    return NotFound(new { error = "Aucun gift attribué à ce dermo dans aucune base." });
                }

                bool isContext1 = giftAssignment1 != null;
                var dbOrders = isContext1 ? _context.GiftOrders : _context2.GiftOrders;
                var assignment = isContext1 ? giftAssignment1 : giftAssignment2;

                // ================================
                // 🆕 Comparaison date sans l'heure
                // ================================
                var existingFull = dbOrders.FirstOrDefault(o =>
                    o.GiftRef == request.GiftRef &&
                    o.DermoId == request.DermoId &&
                    o.DateCommande.Date == request.DateCommande.Date &&
                    o.CodePharmacie == request.CodePharmacie &&
                    o.NomPharmacie == request.NomPharmacie &&
                    o.QuantiteCommande == request.QuantiteCommande
                );

                if (existingFull != null)
                {
                    return BadRequest(new { error = "Ce gift est déjà commandé avec ces paramètres." });
                }

                // ================================
                // 🔄 Vérifier si même ligne avec quantité différente
                // ================================
                var existingDiffQty = dbOrders.FirstOrDefault(o =>
                    o.GiftRef == request.GiftRef &&
                    o.DermoId == request.DermoId &&
                    o.DateCommande.Date == request.DateCommande.Date &&
                    o.CodePharmacie == request.CodePharmacie &&
                    o.NomPharmacie == request.NomPharmacie
                );

                if (existingDiffQty != null && existingDiffQty.QuantiteCommande != request.QuantiteCommande)
                {
                    decimal newQty = request.QuantiteCommande;

                    // 🔹 Vérification stock suffisant
                    if (newQty > assignment.QuantiteAttribuee)
                    {
                        return BadRequest(new
                        {
                            error = $"Stock insuffisant ! Disponible : {assignment.QuantiteAttribuee}"
                        });
                    }

                    // 🔹 Mise à jour stock attribué selon ta logique
                    assignment.QuantiteAttribuee -= newQty;

                    // 🔹 Mise à jour commande existante
                    existingDiffQty.QuantiteCommande = newQty;

                    if (isContext1)
                        _context.SaveChanges();
                    else
                        _context2.SaveChanges();

                    return Ok(new { message = "Quantité mise à jour avec succès." });
                }


                // ================================
                // 4️⃣ Vérification pour nouvelle commande
                // ================================
                if (request.QuantiteCommande <= 0)
                    return BadRequest(new { error = "La quantité commandée doit être > 0." });

                if (request.QuantiteCommande > assignment.QuantiteAttribuee)
                {
                    return BadRequest(new
                    {
                        error = $"Stock insuffisant ! Disponible : {assignment.QuantiteAttribuee}"
                    });
                }

                // Déduction stock pour nouvelle commande
                assignment.QuantiteAttribuee -= request.QuantiteCommande;

                if (assignment.QuantiteAttribuee <= 0)
                {
                    if (isContext1)
                        _context.GiftAssignments.Remove(giftAssignment1);
                    else
                        _context2.GiftAssignments.Remove(giftAssignment2);
                }

                if (isContext1)
                    _context.SaveChanges();
                else
                    _context2.SaveChanges();

                // ================================
                // 5️⃣ INSERT GiftOrder
                // ================================
                var newOrder = new GiftOrder
                {
                    GiftRef = request.GiftRef,
                    DermoId = request.DermoId,
                    QuantiteCommande = request.QuantiteCommande,
                    CodePharmacie = request.CodePharmacie,
                    NomPharmacie = request.NomPharmacie,
                    DateCommande = request.DateCommande
                };

                if (isContext1)
                {
                    _context.GiftOrders.Add(newOrder);
                    _context.SaveChanges();
                }
                else
                {
                    _context2.GiftOrders.Add(newOrder);
                    _context2.SaveChanges();
                }

                return Ok(new
                {
                    message = "Commande enregistrée avec succès.",
                    source = isContext1 ? "Novopharma" : "Medsource",
                    newOrder
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erreur interne, veuillez réessayer plus tard." });
            }
        }




        [HttpGet("gift-orders")]
        public IActionResult GetAllGiftOrders()
        {
            var dermoDict = _context.Authentication
       .Select(x => new { x.id, x.username })
       .ToList() // <-- On force EF à finir sa partie SQL ici
       .GroupBy(d => d.id)
       .ToDictionary(
           g => g.Key,
           g => g.Select(x => x.username).ToList()
       );


            var allGiftOrders = new List<object>();

            // 2. Récupérer les GiftOrders du context1 (Novopharma)
            var giftOrdersContext1 = (from o in _context.GiftOrders
                                      join a in _context.MBAArticle on o.GiftRef equals a.ArRef
                                      select new
                                      {
                                          GiftRef = o.GiftRef,
                                          GiftName = a.ArDesign,
                                          Marque = a.Marque,
                                          Photo = a.Ar_photo,
                                          QuantiteCommande = o.QuantiteCommande,
                                          DateCommande = o.DateCommande,
                                          DermoId = o.DermoId,
                                          DermoName = dermoDict.ContainsKey(o.DermoId)
                                                       ? string.Join(" / ", dermoDict[o.DermoId])
                                                       : null,
                                          CodePharmacie = o.CodePharmacie,
                                          NomPharmacie = o.NomPharmacie,
                                          Source = "Novopharma"
                                      }).ToList();

            allGiftOrders.AddRange(giftOrdersContext1);

            // 3. Récupérer les GiftOrders du context2 (Medsource) SANS utiliser Authentication context2
            var giftOrdersContext2 = (from o in _context2.GiftOrders
                                      join a in _context2.MBAArticle on o.GiftRef equals a.ArRef
                                      select new
                                      {
                                          GiftRef = o.GiftRef,
                                          GiftName = a.ArDesign,
                                          Marque = a.Marque,
                                          Photo = a.Ar_photo,
                                          QuantiteCommande = o.QuantiteCommande,
                                          DateCommande = o.DateCommande,
                                          DermoId = o.DermoId,
                                          DermoName = dermoDict.ContainsKey(o.DermoId)
                                                       ? string.Join(" / ", dermoDict[o.DermoId])
                                                       : null, // ❌ pas trouvé = null
                                          CodePharmacie = o.CodePharmacie,
                                          NomPharmacie = o.NomPharmacie,
                                          Source = "Medsource"
                                      }).ToList();

            allGiftOrders.AddRange(giftOrdersContext2);

            if (!allGiftOrders.Any())
                return NotFound("Aucune commande de gift trouvée.");

            return Ok(allGiftOrders);
        }



        [HttpGet("gift-orders-by-dermo/{dermoId}")]
        public IActionResult GetGiftOrdersByDermo(int dermoId)
        {
            // Requête depuis le premier contexte et matérialisation en mémoire
            var giftOrders1 = (from o in _context.GiftOrders
                               join a in _context.MBAArticle on o.GiftRef equals a.ArRef
                               join d in _context.Authentication on o.DermoId equals d.id
                               where o.DermoId == dermoId
                               select new
                               {
                                   GiftRef = o.GiftRef,
                                   GiftName = a.ArDesign,
                                   Marque = a.Marque,
                                   PrixAchat = a.ArPrixAch,
                                   PrixVente = a.ArPrixVen,
                                   Photo = a.Ar_photo,
                                   QuantiteCommande = o.QuantiteCommande,
                                   DateCommande = o.DateCommande,
                                   DermoId = d.id,
                                   DermoName = d.username,
                                   CodePharmacie = o.CodePharmacie,
                                   NomPharmacie = o.NomPharmacie
                               }).ToList(); // <- exécute la requête ici

            // Requête depuis le deuxième contexte et matérialisation en mémoire
            var giftOrders2 = (from o in _context2.GiftOrders
                               join a in _context2.MBAArticle on o.GiftRef equals a.ArRef
                               join d in _context.Authentication on o.DermoId equals d.id
                               where o.DermoId == dermoId
                               select new
                               {
                                   GiftRef = o.GiftRef,
                                   GiftName = a.ArDesign,
                                   Marque = a.Marque,
                                   PrixAchat = a.ArPrixAch,
                                   PrixVente = a.ArPrixVen, // <-- propriété EF Core
                                   Photo = a.Ar_photo,
                                   QuantiteCommande = o.QuantiteCommande,
                                   DateCommande = o.DateCommande,
                                   DermoId = d.id,
                                   DermoName = d.username,
                                   CodePharmacie = o.CodePharmacie,
                                   NomPharmacie = o.NomPharmacie
                               }).ToList();

            // Concaténer les deux listes en mémoire
            var allGiftOrders = giftOrders1.Concat(giftOrders2).ToList();

            if (!allGiftOrders.Any())
            {
                return NotFound($"Aucune commande trouvée pour le dermo-conseiller ID {dermoId}.");
            }

            return Ok(allGiftOrders);
        }



        [HttpGet("gift-orders-by-dermo")]
        public IActionResult GetGiftOrdersByDermo(
    [FromQuery] string nomDermo = null,
    [FromQuery] string dateCommande = null,
    [FromQuery] string pharmacieId = null)
        {
            // Nettoyage
            nomDermo = nomDermo?.Trim();
            dateCommande = dateCommande?.Trim();
            pharmacieId = pharmacieId?.Trim();

            if (string.IsNullOrEmpty(nomDermo))
                return BadRequest("nomDermo est obligatoire.");

            List<int> dermoIds = new List<int>();

            // 1️⃣ Récupération du dermoId selon nomDermo
            var ids1 = _context.Authentication
                .Where(x => x.username.ToLower() == nomDermo.ToLower())
                .Select(x => x.id)
                .ToList();

            var ids2 = _context2.Authentication
                .Where(x => x.username.ToLower() == nomDermo.ToLower())
                .Select(x => x.id)
                .ToList();

            dermoIds = ids1.Concat(ids2).Distinct().ToList();

            if (!dermoIds.Any())
                return NotFound("Aucun dermo trouvé avec ce nom.");

            // 2️⃣ Construire le dictionnaire NOM → DermoName
            var dermos1 = _context.Authentication
                .Select(x => new { x.id, x.username })
                .ToList();

            var dermos2 = _context2.Authentication
                .Select(x => new { x.id, x.username })
                .ToList();

            var dermoDict = dermos1.Concat(dermos2)
     .GroupBy(d => d.id)
     .ToDictionary(
         g => g.Key,
         g => string.Join(" / ", g.Select(x => x.username).Distinct())
     );


            // 3️⃣ Récupération GiftOrders depuis DB1
            var giftOrders1 = (from o in _context.GiftOrders
                               join a in _context.MBAArticle
                                    on o.GiftRef equals a.ArRef into art
                               from a in art.DefaultIfEmpty()
                               where dermoIds.Contains(o.DermoId)
                               select new
                               {
                                   GiftRef = o.GiftRef,
                                   GiftName = a.ArDesign,
                                   Marque = a.Marque,
                                   Photo = a.Ar_photo,
                                   QuantiteCommande = o.QuantiteCommande,
                                   DateCommande = o.DateCommande,
                                   DermoId = o.DermoId,
                                   DermoName = dermoDict.ContainsKey(o.DermoId) ? dermoDict[o.DermoId] : "",
                                   CodePharmacie = o.CodePharmacie ?? "",
                                   NomPharmacie = o.NomPharmacie ?? "",
                                   Source = "Novopharma"
                               }).ToList();

            // 4️⃣ Récupération GiftOrders depuis DB2
            var giftOrders2 = (from o in _context2.GiftOrders
                               join a in _context2.MBAArticle
                                    on o.GiftRef equals a.ArRef into art
                               from a in art.DefaultIfEmpty()
                               where dermoIds.Contains(o.DermoId)
                               select new
                               {
                                   GiftRef = o.GiftRef,
                                   GiftName = a.ArDesign,
                                   Marque = a.Marque,
                                   Photo = a.Ar_photo,
                                   QuantiteCommande = o.QuantiteCommande,
                                   DateCommande = o.DateCommande,
                                   DermoId = o.DermoId,
                                   DermoName = dermoDict.ContainsKey(o.DermoId) ? dermoDict[o.DermoId] : "",
                                   CodePharmacie = o.CodePharmacie ?? "",
                                   NomPharmacie = o.NomPharmacie ?? "",
                                   Source = "Medsource"
                               }).ToList();

            // 5️⃣ Fusion EN MÉMOIRE
            var allGiftOrders = giftOrders1.Concat(giftOrders2);

            // 6️⃣ Filtre date
            if (!string.IsNullOrEmpty(dateCommande))
            {
                if (!DateTime.TryParseExact(dateCommande, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime dateFilter))
                    return BadRequest("Format date incorrect (dd/MM/yyyy)");

                allGiftOrders = allGiftOrders.Where(x => x.DateCommande.Date == dateFilter.Date);
            }

            // 7️⃣ Filtre pharmacie
            if (!string.IsNullOrEmpty(pharmacieId))
            {
                allGiftOrders = allGiftOrders.Where(x => x.CodePharmacie == pharmacieId);
            }

            var result = allGiftOrders.ToList();

            if (!result.Any())
                return NotFound("Aucune commande trouvée.");

            return Ok(result);
        }



        [HttpGet("gift-orders-by-pharmacie/{codePharmacie}")]
        public IActionResult GetGiftOrdersByPharmacie(string codePharmacie)
        {
            var giftOrders = (from o in _context.GiftOrders
                              join a in _context.MBAArticle on o.GiftRef equals a.ArRef
                              join d in _context.Authentication on o.DermoId equals d.id
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
