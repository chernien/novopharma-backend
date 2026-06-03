using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Linq;
using System.Net;
using WebApiTest.Data;
using WebApiTest.Interface;
using WebApiTest.Models;
using WebApiTest.Repository;

namespace WebApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MsAClientController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;
        private readonly NOVOPHARMAContext _context;
        private readonly MEDSOURCEContext _context2;
        private readonly IMemoryCache _cache;
        public MsAClientController(IClientRepository clientRepository, NOVOPHARMAContext context, MEDSOURCEContext context2, IMemoryCache cache)
        {
            _context = context;
            _clientRepository = clientRepository;
            _context2 = context2;
            _cache = cache; // ✅ affectation

        }
        [HttpPost("login")]
        [ProducesResponseType(200, Type = typeof(MsAClients))]
        public IActionResult onLogin(LoginRequest request)
        {
            var valid_request = _clientRepository.Login(request.username, request.password);
            var client = _clientRepository.GetClientByNum(request.username);
            var userconnect = _clientRepository.GetClientAuthuser(request.username);

            if (valid_request && request.username != "admin-novo")
            {
                return Ok(client);
            }

            else if (valid_request && userconnect.role == "Admin")
            {
                return Ok(userconnect);
            }

            else
            {
                return BadRequest("wrong credentials");
            }

        }

        [HttpPost("check-in")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.LocalisationCheckIn))
                {
                    return BadRequest("Le 'username' et la 'localisation check-in' sont requis.");
                }

                var user = _clientRepository.GetClientAuthuser(request.Username);
                if (user == null)
                {
                    return NotFound($"Utilisateur avec le username '{request.Username}' non trouvé.");
                }

                var updated = _clientRepository.UpdateUserCheckIn(request.Username, request.LocalisationCheckIn, request.DateCheckIn);
                if (!updated)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Erreur lors du check-in.");
                }

                return Ok(new
                {
                    message = $"Check-in effectué pour {request.Username}",
                    localisationCheckIn = request.LocalisationCheckIn,
                    dateCheckIn = request.DateCheckIn
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erreur serveur : {ex.Message}");
            }
        }




        [HttpPost("check-out")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.LocalisationCheckOut))
                {
                    return BadRequest("Le 'username' et la 'localisation check-out' sont requis.");
                }

                var user = _clientRepository.GetClientAuthuser(request.Username);
                if (user == null)
                {
                    return NotFound($"Utilisateur avec le username '{request.Username}' non trouvé.");
                }

                var updated = _clientRepository.UpdateUserCheckOut(request.Username, request.LocalisationCheckOut, request.DateCheckOut);
                if (!updated)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Erreur lors du check-out.");
                }

                return Ok(new
                {
                    message = $"Check-out effectué pour {request.Username}",
                    localisationCheckOut = request.LocalisationCheckOut,
                    dateCheckOut = request.DateCheckOut
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erreur serveur : {ex.Message}");
            }
        }



        [HttpPost("loginComm")]
        [ProducesResponseType(200, Type = typeof(MsAClients))]
        public IActionResult onLoginComm(LoginRequest request)
        {
            var valid_request = _clientRepository.Login(request.username, request.password);

            var userconnect = _clientRepository.GetClientAuthuser(request.username);

            if (valid_request)
            {
               
                return Ok(userconnect);
            }

            else
            {
                return BadRequest("wrong credentials");
            }

        }

        [HttpGet("dermos")]
        public IActionResult GetAllDermos()
        {
            try
            {
                // 🔥 Récupération de tous les utilisateurs avec rôle = Dermo
                var dermos = _context.Authentication
                                     .Where(a => a.role == "Dermo")
                                     .ToList();

                if (dermos == null || dermos.Count == 0)
                {
                    return NotFound("Aucun utilisateur avec le rôle 'Dermo' trouvé.");
                }

                return Ok(dermos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur interne du serveur: {ex.Message}");
            }
        }


        [HttpPost("admin/login")]
        [ProducesResponseType(200, Type = typeof(LoginRequest))]
        public IActionResult onLoginAdmin(LoginRequest request)
        {
            var valid_request = _clientRepository.Login(request.username, request.password);
            var userconnect = _clientRepository.GetClientAuthuser(request.username);
            if (valid_request && userconnect.role == "Admin")
            {
                return Ok(userconnect);
            }
            else
            {
                return BadRequest("wrong credentials");
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetAllClients()
        {
            if (!_cache.TryGetValue("clients", out List<ClientDto> allClients))
            {
                var task1 = _context.MsAClients
                    .AsNoTracking()
                    .Where(c => c.CtNum.StartsWith("C")
                             && c.CtIntitule != null
                             && c.CtQualite == "PHARMACIE") // ✅ AJOUT ICI
                    .Select(c => new ClientDto
                    {
                        CtNum = c.CtNum,
                        CtIntitule = c.CtIntitule,
                        CtQualite = c.CtQualite,
                        CtAdresse = c.CtAdresse
                    })
                    .ToListAsync();

                var task2 = _context2.MsAClients
                    .AsNoTracking()
                    .Where(c => c.CtNum.StartsWith("C")
                             && c.CtIntitule != null
                             && c.CtQualite == "PHARMACIE") // ✅ AJOUT ICI
                    .Select(c => new ClientDto
                    {
                        CtNum = c.CtNum,
                        CtIntitule = c.CtIntitule,
                        CtQualite = c.CtQualite,
                        CtAdresse = c.CtAdresse
                    })
                    .ToListAsync();

                await Task.WhenAll(task1, task2);

                allClients = task1.Result
                    .Concat(task2.Result)
                    .GroupBy(c => (c.CtIntitule ?? "").Trim().ToUpper())
                    .Select(g => g.First())
                    .OrderBy(c => c.CtIntitule)
                    .ToList();

                _cache.Set("clients", allClients, TimeSpan.FromMinutes(10));
            }

            return Ok(allClients);
        }



        [HttpPut("UpdateEnabled/{id}")]
        public IActionResult UpdateEnabledStatus(int id, [FromBody] bool enabled)
        {
            try
            {
                var dermo = _context.Authentication.FirstOrDefault(a => a.id == id);

                if (dermo == null)
                    return NotFound(new { message = $"Aucun Dermo trouvé avec l'ID {id}." });

                dermo.enabled = enabled;
                _context.SaveChanges();

                return Ok(new
                {
                    message = $"✅ Statut du Dermo (ID={id}) mis à jour avec succès.",
                    newStatus = enabled
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "❌ Erreur lors de la mise à jour du statut.", error = ex.Message });
            }
        }






        [HttpGet("auth")]
        [ProducesResponseType(200, Type = typeof(ICollection<LoginRequest>))]
        public IActionResult GetAllClientsAuth()
        {
            var clientsAuth = _clientRepository.GetClientsAuth();
            return Ok(clientsAuth);
        }
        [HttpGet("/auth/{id}")]
        [ProducesResponseType(200, Type = typeof(LoginRequest))]
        public IActionResult GetClientAuth(int id)
        {
            var clientAuth = _clientRepository.GetClientAuth(id);
            return Ok(clientAuth);
        }
        [HttpPut("/auth/update/{id}")]
        public async Task<ActionResult<LoginRequest>> UpdateEmployeeEnabled(int id)
        {
            try
            {
                var clientAuthToUpdate = _clientRepository.GetClientAuth(id);

                if (clientAuthToUpdate == null)
                {
                    return NotFound($"Client with Id = {id} not found");
                }


                if (clientAuthToUpdate.enabled == false)
                {
                    clientAuthToUpdate.enabled = true;
                }
                else
                {
                    clientAuthToUpdate.enabled = false;
                }
                _clientRepository.UpdateUserAuth(clientAuthToUpdate);
                return Ok(clientAuthToUpdate);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data");
            }
        }
        [HttpPut("/auth/password/{id}")]
        public async Task<ActionResult<LoginRequest>> UpdateEmployee(int id, [FromBody] LoginRequest userAuth)
        {
            try
            {
                var clientAuthToUpdate = _clientRepository.GetClientAuth(id);

                if (clientAuthToUpdate == null)
                {
                    return NotFound($"Client with Id = {id} not found");
                }

                clientAuthToUpdate.password = userAuth.password;
                clientAuthToUpdate.intitule = userAuth.intitule;
                clientAuthToUpdate.username = userAuth.username;

                _clientRepository.UpdateUserAuth(clientAuthToUpdate);
                return Ok(clientAuthToUpdate);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data");
            }
        }

        [HttpPost("dermo/add")]
        [ProducesResponseType(200, Type = typeof(LoginRequest))]
        public async Task<IActionResult> AddNewClientsAsync([FromBody] LoginRequest userAuth)
        {
            var clients = await _context.MsAClients.Where(c => !_context.Authentication.Any(nt => nt.username == c.CtNum)).ToListAsync();


            var random = new Random();
            var password = Guid.NewGuid().ToString("N").Substring(0, 10); ;

            var nouvelleEntree = new LoginRequest
            {
                username = userAuth.username,
                intitule = userAuth.intitule,
                password = userAuth.password,
                role = "Dermo",
                enabled = true,
            };
            _context.Authentication.Add(nouvelleEntree);

            await _context.SaveChangesAsync();
            return Ok(nouvelleEntree);
        }
        [HttpPost("admin/add")]
        [ProducesResponseType(200, Type = typeof(LoginRequest))]
        public async Task<IActionResult> AddNewDermoAsync()
        {
            var clients = await _context.MsAClients.Where(c => !_context.Authentication.Any(nt => nt.username == c.CtNum)).ToListAsync();

            foreach (var client in clients)
            {
                var random = new Random();
                var password = Guid.NewGuid().ToString("N").Substring(0, 10); ;

                var nouvelleEntree = new LoginRequest
                {
                    username = client.CtNum,
                    intitule = client.CtIntitule,
                    password = password,
                    enabled = true,
                };
                _context.Authentication.Add(nouvelleEntree);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPost("admin/removeDuplicates")]
        public async Task<IActionResult> RemoveDuplicateEntriesAsync()
        {
            // Regrouper les entrées par username et sélectionner uniquement la première entrée de chaque groupe
            var distinctEntries = _context.Authentication
                .GroupBy(entry => entry.username)
                .Select(group => group.First())
                .ToList();

            // Supprimer toutes les entrées actuelles de la table Authentication
            _context.Authentication.RemoveRange(_context.Authentication);

            // Ajouter les entrées uniques à partir de la liste distinctEntries
            _context.Authentication.AddRange(distinctEntries);

            // Enregistrer les modifications dans la base de données
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpPut("sync/add")]
        [ProducesResponseType(200, Type = typeof(LoginRequest))]
        public async Task<IActionResult> updateAsync()
        {
            var clients = await _context.Authentication.ToListAsync();

            foreach (var client in clients)
            {


                var nouvelleEntree = new LoginRequest
                {
                    username = client.username,
                    password = client.password,
                    enabled = true,
                    intitule = client.intitule,
                    role = "User"
                };
                _context.Authentication.Update(nouvelleEntree);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpGet("commande/one/{id}")]

        public IActionResult GetCommandeOnId(int id)
        {
            var commande = _context.Commandes.FirstOrDefault(c => c.Id == id);
            return Ok(commande);
        }
        [HttpGet("commande")]
        public async Task<IActionResult> GetCommandeArticleSafe(
       [FromQuery] string? date = null,
       [FromQuery] string? dermo = null,
       [FromQuery] string? pharmacyName = null)
        {
            try
            {
                // 🔹 Parse la date si fournie
                DateTime? parsedDate = null;
                if (!string.IsNullOrEmpty(date))
                {
                    if (DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    {
                        parsedDate = dt;
                    }
                    else
                    {
                        return BadRequest("Format de date invalide. Utiliser dd/MM/yyyy");
                    }
                }

                // 🔹 Récupère les commandes filtrées par date et dermo (CreatedBy)
                var commandesQuery = _context.Commandes
                    .Where(c =>
                        (!parsedDate.HasValue || c.DateCreated.Date == parsedDate.Value.Date) &&
                        (string.IsNullOrEmpty(dermo) || c.CreatedBy == dermo)
                    )
                    .Select(c => new
                    {
                        c.Id,
                        c.Quantity,
                        c.QuantityVendue,
                        c.DateCreated,
                        c.LastUpdated,
                        c.CreatedBy,
                        c.isValid,
                        c.PharmacyId,
                        c.ArticleId,
                        c.Source,
                        c.PrixVente,
                        c.ImagePath,
                        c.DatePeremtion,
                        c.DatePeremtion1,
                        c.DatePeremtion2,
                        c.NumSerie,
                        c.NumSerie1,
                        c.NumSerie2,
                        c.Qte1,
                        c.Qte2,
                        c.Qte3
                    });

                var commandes = await commandesQuery.ToListAsync();

                if (!commandes.Any())
                    return Ok(new List<CommandeViewModelDto>());

                var pharmacyIds = commandes.Select(c => c.PharmacyId).Distinct().ToList();

                // 🔹 Récupération des pharmacies depuis les deux contextes
                var pharmacies1 = await _context.MsAClients
                    .Where(p => pharmacyIds.Contains(p.CtNum))
                    .ToListAsync();

                var pharmacies2 = await _context2.MsAClients
                    .Where(p => pharmacyIds.Contains(p.CtNum))
                    .ToListAsync();

                // 🔹 Fusion + suppression des doublons
                var allPharmacies = pharmacies1.Concat(pharmacies2)
                                              .GroupBy(p => p.CtNum)
                                              .Select(g => g.First())
                                              .ToList();

                // 🔹 Filtrage par nom de pharmacie si fourni
                if (!string.IsNullOrEmpty(pharmacyName))
                {
                    allPharmacies = allPharmacies
                        .Where(p => !string.IsNullOrEmpty(p.CtIntitule) &&
                                    p.CtIntitule.Contains(pharmacyName, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                var pharmaciesDict = allPharmacies.ToDictionary(p => p.CtNum, p => p);

                // 🔹 Gestion des articles
                var novopharmaArticleIds = new List<string>();
                var medsourceArticleIds = new List<string>();

                foreach (var c in commandes)
                {
                    var source = (c.Source ?? "Novopharma").Trim();
                    var articleId = (c.ArticleId ?? "").Trim();
                    if (!string.IsNullOrEmpty(articleId))
                    {
                        if (source.Equals("Medsource", StringComparison.OrdinalIgnoreCase))
                            medsourceArticleIds.Add(articleId.ToLower());
                        else
                            novopharmaArticleIds.Add(articleId.ToLower());
                    }
                }

                var articlesDict = new Dictionary<string, object>();

                if (novopharmaArticleIds.Any())
                {
                    var novopharmaArticles = await _context.MsAArticle
                        .Where(a => !string.IsNullOrEmpty(a.ArRef) && novopharmaArticleIds.Contains(a.ArRef.Trim().ToLower()))
                        .ToListAsync();

                    foreach (var a in novopharmaArticles)
                        articlesDict[a.ArRef.Trim().ToLower()] = a;
                }

                if (medsourceArticleIds.Any() && _context2 != null)
                {
                    var medsourceArticles = await _context2.MsSArticle
                        .Where(a => !string.IsNullOrEmpty(a.ArRef) && medsourceArticleIds.Contains(a.ArRef.Trim().ToLower()))
                        .ToListAsync();

                    foreach (var a in medsourceArticles)
                        articlesDict[a.ArRef.Trim().ToLower()] = a;
                }

                // 🔹 Construction finale
                var commandesAvecDetails = new List<CommandeViewModelDto>();

                foreach (var c in commandes)
                {
                    object pharmacyObj;
                    if (pharmaciesDict.TryGetValue(c.PharmacyId, out var pharmacy))
                    {
                        pharmacyObj = pharmacy;
                    }
                    else
                    {
                        pharmacyObj = new { CtNum = c.PharmacyId, Message = "❌ Pharmacy not found" };
                    }

                    string source = (c.Source ?? "Novopharma").Trim();
                    string articleId = (c.ArticleId ?? "").Trim().ToLower();

                    object articleObj = articlesDict.TryGetValue(articleId, out var article)
                        ? article
                        : new { Message = $"❌ Article {c.ArticleId} not found in {source}" };

                    commandesAvecDetails.Add(new CommandeViewModelDto
                    {
                        Id = c.Id,
                        Quantity = c.Quantity,
                        QuantityVendue = c.QuantityVendue,
                        DateCreated = c.DateCreated,
                        LastUpdated = c.LastUpdated,
                        CreatedBy = c.CreatedBy,
                        isValid = c.isValid,
                        pharmacy = pharmacyObj,
                        article = articleObj,
                        PrixVente = c.PrixVente,
                        ImagePath = c.ImagePath,
                        DatePeremtion = c.DatePeremtion,
                        DatePeremtion1 = c.DatePeremtion1,
                        DatePeremtion2 = c.DatePeremtion2,
                        NumSerie = c.NumSerie,
                        NumSerie1 = c.NumSerie1,
                        NumSerie2 = c.NumSerie2,
                        Qte1 = c.Qte1,
                        Qte2 = c.Qte2,
                        Qte3 = c.Qte3
                    });
                }

                return Ok(commandesAvecDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 ERREUR GLOBALE: {ex.Message}");
                Console.WriteLine($"🔥 STACK TRACE: {ex.StackTrace}");
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("CheckCommande")]
        public async Task<IActionResult> CheckCommande(
     string pharmacyId,
     string articleId,
     DateTime dateCreated,
     string createdBy,
     int quantity,
     int quantityVendue)
        {
            // On ignore l’heure → comparaison uniquement sur la date
            var dateOnly = dateCreated.Date;

            var commande = await _context.Commandes
                .Where(c =>
                    c.PharmacyId == pharmacyId &&
                    c.ArticleId == articleId &&
                    c.CreatedBy == createdBy &&
                    c.DateCreated.Date == dateOnly)
                .FirstOrDefaultAsync();

            if (commande == null)
                return Ok(new { exists = false });

            // 🔥 CAS 1 → EXACTEMENT identique
            if (commande.Quantity == quantity &&
                commande.QuantityVendue == quantityVendue)
            {
                return Ok(new
                {
                    exists = true,
                    message = "Cet article est déjà commandé."
                });
            }

            // 🔥 CAS 2 → Quantity différente
            bool quantityMismatch = commande.Quantity != quantity;

            // 🔥 CAS 3 → QuantityVendue différente
            bool quantityVendueMismatch = commande.QuantityVendue != quantityVendue;

            // 🔥 CAS 4 → Les deux différentes
            if (quantityMismatch && quantityVendueMismatch)
            {
                return Ok(new
                {
                    exists = true,
                    mismatch = "both_false",
                    message = "Quantity et QuantityVendue ne correspondent pas."
                });
            }

            // Si juste quantity FAUSSE
            if (quantityMismatch)
            {
                return Ok(new
                {
                    exists = true,
                    mismatch = "quantity_false",
                    message = "Quantity ne correspond pas."
                });
            }

            // Si juste quantityVendue FAUSSE
            if (quantityVendueMismatch)
            {
                return Ok(new
                {
                    exists = true,
                    mismatch = "quantityVendue_false",
                    message = "QuantityVendue ne correspond pas."
                });
            }

            // Sécurité
            return Ok(new { exists = true });
        }



        // ✅ GET: api/ComFact
        [HttpGet("comfact")]
        public async Task<IActionResult> GetComFact(
            [FromQuery] string? dateArrivee = null,
            [FromQuery] string? createdBy = null // ⚡ renommé pour être plus explicite
        )
        {
            try
            {
                // 🔹 1. Vérification et parsing de la date
                DateTime? parsedDate = null;
                if (!string.IsNullOrEmpty(dateArrivee))
                {
                    if (DateTime.TryParseExact(dateArrivee, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        parsedDate = dt;
                    else
                        return BadRequest("Format de date invalide. Utiliser dd/MM/yyyy");
                }

                // 🔹 2. Filtrage sur DateArrivee et CreatedBy (le nom du dermato)
                var commandesQuery = _context.ComFacts
                    .Where(c =>
                        (!parsedDate.HasValue || (c.DateArrivee.HasValue && c.DateArrivee.Value.Date == parsedDate.Value.Date)) &&
                        (string.IsNullOrEmpty(createdBy) || (c.CreatedBy != null && c.CreatedBy.Contains(createdBy)))
                    )
                    .Select(c => new
                    {
                        c.Id,
                        c.PharmacyId,
                        c.ArticleId,
                        c.Quantity,
                        c.QuantityVendue,
                        c.DateCreated,
                        c.CreatedBy,
                        c.PrixVente,
                        c.ImagePath,
                        c.DatePérumption,
                        c.DatePérumption1,
                        c.DatePérumption2,
                        c.Qte1,
                        c.Qte2,
                        c.Qte3,
                        c.Source,
                        c.FactureId,
                        c.code_pharmacie,
                        c.DateArrivee
                    });

                var commandes = await commandesQuery.ToListAsync();

                // ✅ Si aucune commande trouvée
                if (!commandes.Any())
                    return Ok(new List<CommandeViewModelDto2>());

                // 🔹 3. Récupération des pharmacies (depuis les 2 bases)
                var pharmacyIds = commandes
                    .Select(c => c.PharmacyId)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .ToList();

                var pharmacies1 = await _context.MsAClients
                    .Where(p => pharmacyIds.Contains(p.CtNum))
                    .ToListAsync();

                var pharmacies2 = await _context2.MsAClients
                    .Where(p => pharmacyIds.Contains(p.CtNum))
                    .ToListAsync();

                var allPharmacies = pharmacies1
                    .Concat(pharmacies2)
                    .GroupBy(p => p.CtNum)
                    .Select(g => g.First())
                    .ToList();

                var pharmaciesDict = allPharmacies.ToDictionary(p => p.CtNum, p => p);

                // 🔹 4. Gestion des articles selon la source
                var novopharmaArticleIds = new List<string>();
                var medsourceArticleIds = new List<string>();

                foreach (var c in commandes)
                {
                    var source = (c.Source ?? "Novopharma").Trim();
                    var articleId = (c.ArticleId ?? "").Trim().ToLower();

                    if (string.IsNullOrEmpty(articleId)) continue;

                    if (source.Equals("Medsource", StringComparison.OrdinalIgnoreCase))
                        medsourceArticleIds.Add(articleId);
                    else
                        novopharmaArticleIds.Add(articleId);
                }

                var articlesDict = new Dictionary<string, object>();

                // 🔹 Articles de Novopharma
                if (novopharmaArticleIds.Any())
                {
                    var novopharmaArticles = await _context.MsAArticle
                        .Where(a => !string.IsNullOrEmpty(a.ArRef) && novopharmaArticleIds.Contains(a.ArRef.Trim().ToLower()))
                        .ToListAsync();

                    foreach (var a in novopharmaArticles)
                        articlesDict[a.ArRef.Trim().ToLower()] = a;
                }

                // 🔹 Articles de Medsource
                if (medsourceArticleIds.Any() && _context2 != null)
                {
                    var medsourceArticles = await _context2.MsSArticle
                        .Where(a => !string.IsNullOrEmpty(a.ArRef) && medsourceArticleIds.Contains(a.ArRef.Trim().ToLower()))
                        .ToListAsync();

                    foreach (var a in medsourceArticles)
                        articlesDict[a.ArRef.Trim().ToLower()] = a;
                }

                // 🔹 5. Construction du résultat final
                var commandesAvecDetails = commandes.Select(c =>
                {
                    object pharmacyObj = pharmaciesDict.TryGetValue(c.PharmacyId, out var pharmacy)
                        ? pharmacy
                        : new { CtNum = c.PharmacyId, Message = "❌ Pharmacy not found" };

                    string source = (c.Source ?? "Novopharma").Trim();
                    string articleId = (c.ArticleId ?? "").Trim().ToLower();

                    object articleObj = articlesDict.TryGetValue(articleId, out var article)
                        ? article
                        : new { Message = $"❌ Article {c.ArticleId} not found in {source}" };

                    return new CommandeViewModelDto2
                    {
                        Id = c.Id,
                        Quantity = c.Quantity,
                        QuantityVendue = c.QuantityVendue,
                        DateCreated = c.DateCreated,
                        CreatedBy = c.CreatedBy,
                        pharmacy = pharmacyObj,
                        article = articleObj,
                        PrixVente = c.PrixVente,
                        ImagePath = c.ImagePath,
                        DatePeremtion = c.DatePérumption,
                        DatePeremtion1 = c.DatePérumption1,
                        DatePeremtion2 = c.DatePérumption2,
                        Qte1 = c.Qte1,
                        Qte2 = c.Qte2,
                        Qte3 = c.Qte3,
                        FactureId = c.FactureId,
                        CodePharmacie = c.code_pharmacie,
                        DateArrivee = c.DateArrivee
                    };
                }).ToList();

                return Ok(commandesAvecDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 ERREUR GLOBALE: {ex.Message}");
                Console.WriteLine($"🔥 STACK TRACE: {ex.StackTrace}");
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("commande/filtre")]
        public IActionResult GetCommandeArticleFiltré(string? pharmacieId, string? createdBy, DateTime? date)
        {
            if (_context2 == null)
            {
                return StatusCode(500, "❌ Internal Server Error: Medsource Database Context is NULL.");
            }

            var commandes = _context.Commandes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(pharmacieId))
                commandes = commandes.Where(c => c.PharmacyId == pharmacieId);

            if (!string.IsNullOrWhiteSpace(createdBy))
                commandes = commandes.Where(c => c.CreatedBy == createdBy);

            if (date.HasValue)
            {
                var startOfDay = date.Value.Date;
                var endOfDay = date.Value.Date.AddDays(1).AddTicks(-1);
                commandes = commandes.Where(c => c.DateCreated >= startOfDay && c.DateCreated <= endOfDay);
            }

            var commandesAvecDetails = new List<CommandeViewModelDto>();

            foreach (var commande in commandes)
            {
                var pharmacy = _context.MsAClients.FirstOrDefault(p => p.CtNum == commande.PharmacyId);
                object? article;

                article = commande.Source == "Medsource"
                    ? _context2.MsSArticle.FirstOrDefault(a => a.ArRef != null && a.ArRef.Trim() == commande.ArticleId)
                    : _context.MsAArticle.FirstOrDefault(a => a.ArRef != null && a.ArRef.Trim() == commande.ArticleId);

                var commandeAvecDetails = new CommandeViewModelDto
                {
                    Id = commande.Id,
                    Quantity = commande.Quantity,
                    QuantityVendue = commande.QuantityVendue,
                    DateCreated = commande.DateCreated,
                    LastUpdated = commande.LastUpdated,
                    CreatedBy = commande.CreatedBy,
                    isValid = commande.isValid,
                    pharmacy = pharmacy!,
                    article = article ?? new { Message = $"❌ Article {commande.ArticleId} not found in {commande.Source}" },
                    PrixVente = commande.PrixVente,
                    ImagePath = commande.ImagePath,
                    DatePeremtion = commande.DatePeremtion,
                    DatePeremtion1 = commande.DatePeremtion1,
                    DatePeremtion2 = commande.DatePeremtion2,
                    NumSerie = commande.NumSerie,
                    NumSerie1 = commande.NumSerie1,
                    NumSerie2 = commande.NumSerie2,
                    Qte1 = commande.Qte1,
                    Qte2 = commande.Qte2,
                    Qte3 = commande.Qte3,
                };

                commandesAvecDetails.Add(commandeAvecDetails);
            }

            return Ok(commandesAvecDetails);
        }

        [HttpGet("commandeP/{pharmacieId}")]
        [ProducesResponseType(200, Type = typeof(ICollection<CommandeViewModelDto>))]
        public IActionResult GetCommandePharmacie(string pharmacieId)
        {
            var commandesAvecDetails = _context.Commandes
                .Where(c => c.PharmacyId == pharmacieId)
                .Select(c => new CommandeViewModelDto
                {
                    Id = c.Id,
                    Quantity = c.Quantity,
                    QuantityVendue = c.QuantityVendue,
                    DateCreated = c.DateCreated,
                    LastUpdated = c.LastUpdated,
                    CreatedBy = c.CreatedBy,
                    isValid = c.isValid,
                    pharmacy = _context.MsAClients
                        .Where(p => p.CtNum == c.PharmacyId)
                        .Select(p => new PharmacieCommandeDto // Création d'un DTO minimal pour la pharmacie
                        {
                            CtNum = p.CtNum,
                            CtIntitule = p.CtIntitule
                        })
                        .FirstOrDefault(),
                    article = _context.MsAArticle
                        .Where(a => a.ArRef == c.ArticleId)
                        .Select(a => new ArticleCommandeDto // Création d'un DTO minimal pour l'article
                        {
                            ArRef = a.ArRef,
                            ArDesign = a.ArDesign
                        })
                        .FirstOrDefault(),
                    PrixVente = c.PrixVente,
                    ImagePath = c.ImagePath,
                    DatePeremtion = c.DatePeremtion,
                    DatePeremtion1 = c.DatePeremtion1,
                    DatePeremtion2 = c.DatePeremtion2,
                    NumSerie = c.NumSerie,
                    NumSerie1 = c.NumSerie1,
                    NumSerie2 = c.NumSerie2,
                    Qte1 = c.Qte1,
                    Qte2 = c.Qte2,
                    Qte3 = c.Qte3
                })
                .AsNoTracking() // Optimisation pour lecture seule
                .ToList();

            return Ok(commandesAvecDetails);
        }



        [HttpGet("commandeA/{articleId}")]
        [ProducesResponseType(200, Type = typeof(ICollection<Commande>))]
        public IActionResult GetCommandeArticle(string articleId)
        {
            var commandes = _context.Commandes.Where(c => c.ArticleId == articleId).ToList();

            var commandesAvecDetails = new List<CommandeViewModelDto>();

            foreach (var commande in commandes)
            {
                // Récupérer les détails complets de la pharmacie et de l'article pour chaque commande
                var pharmacy = _context.MsAClients.FirstOrDefault(p => p.CtNum == commande.PharmacyId);
                var article = _context.MsAArticle.FirstOrDefault(a => a.ArRef == commande.ArticleId);

                // Créer un ViewModel avec les détails complets
                var commandeAvecDetails = new CommandeViewModelDto
                {
                    Id = commande.Id,
                    Quantity = commande.Quantity,
                    QuantityVendue = commande.QuantityVendue,
                    DateCreated = commande.DateCreated,
                    LastUpdated = commande.LastUpdated,
                    CreatedBy = commande.CreatedBy,
                    isValid = commande.isValid,
                    pharmacy = pharmacy,
                    article = article,
                    PrixVente = commande.PrixVente,
                    ImagePath = commande.ImagePath


                };

                commandesAvecDetails.Add(commandeAvecDetails);
            }

            return Ok(commandesAvecDetails);
        }
        [HttpGet("commandeA/{pharmacieId}/{articleId}")]
        [ProducesResponseType(200, Type = typeof(ICollection<Commande>))]
        public IActionResult GetCommandeArticle(string pharmacieId, string articleId)
        {
            var commandes = _context.Commandes.Where(c => c.ArticleId == articleId && c.PharmacyId == pharmacieId).ToList();

            var commandesAvecDetails = new List<CommandeViewModelDto>();

            foreach (var commande in commandes)
            {
                // Récupérer les détails complets de la pharmacie et de l'article pour chaque commande
                var pharmacy = _context.MsAClients.FirstOrDefault(p => p.CtNum == commande.PharmacyId);
                var article = _context.MsAArticle.FirstOrDefault(a => a.ArRef == commande.ArticleId);

                // Créer un ViewModel avec les détails complets
                var commandeAvecDetails = new CommandeViewModelDto
                {
                    Id = commande.Id,
                    Quantity = commande.Quantity,
                    QuantityVendue = commande.QuantityVendue,
                    DateCreated = commande.DateCreated,
                    LastUpdated = commande.LastUpdated,
                    CreatedBy = commande.CreatedBy,
                    isValid = commande.isValid,
                    pharmacy = pharmacy,
                    article = article,
                    PrixVente = commande.PrixVente,
                    ImagePath = commande.ImagePath



                };

                commandesAvecDetails.Add(commandeAvecDetails);
            }

            return Ok(commandesAvecDetails);
        }
        [HttpPost("commandeComm")]
        public async Task<ActionResult> PostCommande(
    [FromBody] CommandeViewModel commandeViewModel,
    string commercial,
    string source)
        {
            try
            {
                if (commandeViewModel == null || string.IsNullOrEmpty(commercial) || string.IsNullOrEmpty(source))
                    return BadRequest("❌ Les données de commande ou la source sont invalides.");

                if (source != "Novopharma" && source != "Medsource")
                    return BadRequest("❌ Source invalide. Utilisez 'Novopharma' ou 'Medsource'.");

                // Vérifier si l'article existe dans la base (source)
                bool articleExists = source == "Novopharma"
                    ? await _context.MsAArticle.AnyAsync(a => a.ArRef == commandeViewModel.article.ArRef)
                    : await _context2.MsSArticle.AnyAsync(a => a.ArRef == commandeViewModel.article.ArRef);

                if (!articleExists)
                    return NotFound($"❌ Article '{commandeViewModel.article.ArRef}' introuvable dans {source}.");

                // === 🔍 VÉRIFICATION AVANT AJOUT ===
                string pharmacyId = commandeViewModel.pharmacy.CtNum;
                string articleId = commandeViewModel.article.ArRef;
                DateTime dateOnly = commandeViewModel.Datecreated.Date;

                var existingCommande = await _context.Commandes
                    .Where(c =>
                        c.PharmacyId == pharmacyId &&
                        c.ArticleId == articleId &&
                        c.CreatedBy == commercial &&
                        c.DateCreated.Date == dateOnly)
                    .FirstOrDefaultAsync();

                // === 🟥 CAS 1 : La même commande existe EXACTEMENT (les 6 paramètres identiques)
                if (existingCommande != null &&
                    existingCommande.Quantity == commandeViewModel.Quantity &&
                    existingCommande.QuantityVendue == commandeViewModel.QuantityVendue)
                {
                    return Conflict(new
                    {
                        message = "❌ Cet article est déjà commandé.",
                        status = "already_exists"
                    });
                }

                // === 🟧 CAS 2 : Ligne trouvée MAIS quantités différentes → on met à jour
                if (existingCommande != null)
                {
                    bool updated = false;

                    if (existingCommande.Quantity != commandeViewModel.Quantity)
                    {
                        existingCommande.Quantity = commandeViewModel.Quantity;
                        updated = true;
                    }

                    if (existingCommande.QuantityVendue != commandeViewModel.QuantityVendue)
                    {
                        existingCommande.QuantityVendue = commandeViewModel.QuantityVendue;
                        updated = true;
                    }

                    if (updated)
                    {
                        existingCommande.LastUpdated = DateTime.UtcNow;
                        await _context.SaveChangesAsync();

                        return Ok(new
                        {
                            message = "⚠️ Quantités mises à jour pour cette commande existante.",
                            status = "updated",
                            commandeId = existingCommande.Id
                        });
                    }
                }

                // === 🟩 CAS 3 : Nouvelle commande → on l'ajoute normalement
                var commande = new Commande
                {
                    PharmacyId = pharmacyId,
                    ArticleId = articleId,
                    Quantity = commandeViewModel.Quantity,
                    QuantityVendue = commandeViewModel.QuantityVendue,
                    DateCreated = commandeViewModel.Datecreated,
                    LastUpdated = DateTime.UtcNow,
                    CreatedBy = commercial,
                    Source = source,

                    DatePeremtion = commandeViewModel.DatePeremtion,
                    NumSerie = commandeViewModel.NumSerie,
                    DatePeremtion1 = commandeViewModel.DatePeremtion1,
                    NumSerie1 = commandeViewModel.NumSerie1,
                    DatePeremtion2 = commandeViewModel.DatePeremtion2,
                    NumSerie2 = commandeViewModel.NumSerie2,
                    Qte1 = commandeViewModel.Qte1,
                    Qte2 = commandeViewModel.Qte2,
                    Qte3 = commandeViewModel.Qte3
                };

                _context.Commandes.Add(commande);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(PostCommande), new { id = commande.Id }, new
                {
                    message = "✅ Commande créée avec succès !",
                    status = "created",
                    commandeId = commande.Id
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"❌ Erreur SQL : {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "❌ Une erreur s'est produite lors de la création de la commande.");
            }
        }



        [HttpPost("commandeMultiple")]
        public async Task<ActionResult> PostCommandes(
    [FromBody] List<CommandeViewModel> commandes,
    string commercial,
    string source)
        {
            try
            {
                if (commandes == null || commandes.Count == 0 ||
                    string.IsNullOrEmpty(commercial) || string.IsNullOrEmpty(source))
                {
                    return BadRequest(new
                    {
                        errorType = "INVALID_DATA",
                        message = "❌ Les données envoyées sont invalides."
                    });
                }

                if (source != "Novopharma" && source != "Medsource")
                {
                    return BadRequest(new
                    {
                        errorType = "INVALID_SOURCE",
                        message = "❌ Source invalide. Utilisez 'Novopharma' ou 'Medsource'."
                    });
                }

                var result = new List<object>();

                foreach (var cmd in commandes)
                {
                    bool exists = source == "Novopharma"
                        ? await _context.MsAArticle.AnyAsync(a => a.ArRef == cmd.article.ArRef)
                        : await _context2.MsSArticle.AnyAsync(a => a.ArRef == cmd.article.ArRef);

                    if (!exists)
                    {
                        result.Add(new
                        {
                            article = cmd.article.ArRef,
                            status = "❌ Article introuvable",
                            action = "ignored"
                        });
                        continue;
                    }

                    var dateOnly = cmd.Datecreated.Date;

                    var existing = await _context.Commandes.FirstOrDefaultAsync(c =>
                        c.PharmacyId == cmd.pharmacy.CtNum &&
                        c.ArticleId == cmd.article.ArRef &&
                        c.CreatedBy == commercial &&
                        c.DateCreated.Date == dateOnly
                    );

                    if (existing != null)
                    {
                        if (existing.Quantity == cmd.Quantity &&
                            existing.QuantityVendue == cmd.QuantityVendue)
                        {
                            result.Add(new
                            {
                                article = cmd.article.ArRef,
                                status = "❌ Cet article est déjà commandé",
                                action = "ignored"
                            });
                            continue;
                        }

                        bool updated = false;

                        if (existing.Quantity != cmd.Quantity)
                        {
                            existing.Quantity = cmd.Quantity;
                            updated = true;
                        }

                        if (existing.QuantityVendue != cmd.QuantityVendue)
                        {
                            existing.QuantityVendue = cmd.QuantityVendue;
                            updated = true;
                        }

                        if (updated)
                        {
                            existing.LastUpdated = DateTime.UtcNow;
                            result.Add(new
                            {
                                article = cmd.article.ArRef,
                                status = "⚠️ Quantité mise à jour",
                                action = "updated"
                            });
                        }

                        continue;
                    }

                    var newCmd = new Commande
                    {
                        PharmacyId = cmd.pharmacy.CtNum,
                        ArticleId = cmd.article.ArRef,
                        Quantity = cmd.Quantity,
                        QuantityVendue = cmd.QuantityVendue,
                        DateCreated = cmd.Datecreated,
                        LastUpdated = DateTime.UtcNow,
                        CreatedBy = commercial,
                        Source = source,
                        DatePeremtion = cmd.DatePeremtion,
                        NumSerie = cmd.NumSerie,
                        DatePeremtion1 = cmd.DatePeremtion1,
                        NumSerie1 = cmd.NumSerie1,
                        DatePeremtion2 = cmd.DatePeremtion2,
                        NumSerie2 = cmd.NumSerie2,
                        Qte1 = cmd.Qte1,
                        Qte2 = cmd.Qte2,
                        Qte3 = cmd.Qte3,
                    };

                    _context.Commandes.Add(newCmd);

                    result.Add(new
                    {
                        article = cmd.article.ArRef,
                        status = "✅ Inséré",
                        action = "inserted"
                    });
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "🚀 Traitement terminé",
                    details = result
                });
            }
            catch (DbUpdateException dbEx)
            {
                // Erreur lors du SaveChangesAsync (contrainte SQL, violation clé, etc.)
                var detail = dbEx.InnerException?.Message ?? dbEx.Message;
                return StatusCode(500, new
                {
                    errorType = "DATABASE_SAVE_ERROR",
                    message = "❌ Erreur lors de l'enregistrement en base de données.",
                    detail
                });
            }
            catch (Exception ex)
            {
                var exTypeName = ex.GetType().FullName ?? "";
                var innerTypeName = ex.InnerException?.GetType().FullName ?? "";
                var innerMessage = ex.InnerException?.Message ?? "";

                bool isSqlConnection =
                    exTypeName.Contains("SqlException") ||
                    innerTypeName.Contains("SqlException") ||
                    innerMessage.Contains("SQL Server") ||
                    innerMessage.Contains("server was not found") ||
                    innerMessage.Contains("Cannot open database") ||
                    innerMessage.Contains("A network-related");

                if (isSqlConnection)
                {
                    return StatusCode(500, new
                    {
                        errorType = "DATABASE_CONNECTION_ERROR",
                        message = "❌ Impossible de se connecter à la base de données. Vérifiez que SQL Server est démarré.",
                        detail = innerMessage
                    });
                }

                return StatusCode(500, new
                {
                    errorType = "SERVER_ERROR",
                    message = "❌ Erreur interne du serveur.",
                    detail = ex.Message
                });
            }
        }


        //    [HttpPost("commandeMultiple")]
        //    public async Task<ActionResult> PostCommandes(
        //[FromBody] List<CommandeViewModel> commandes,
        //string commercial,
        //string source)
        //    {
        //        try
        //        {
        //            if (commandes == null || commandes.Count == 0 ||
        //                string.IsNullOrEmpty(commercial) || string.IsNullOrEmpty(source))
        //            {
        //                return BadRequest("❌ Les données envoyées sont invalides.");
        //            }

        //            if (source != "Novopharma" && source != "Medsource")
        //            {
        //                return BadRequest("❌ Source invalide. Utilisez 'Novopharma' ou 'Medsource'.");
        //            }

        //            var result = new List<object>();

        //            foreach (var cmd in commandes)
        //            {
        //                // Vérifier article
        //                bool exists = source == "Novopharma"
        //                    ? await _context.MsAArticle.AnyAsync(a => a.ArRef == cmd.article.ArRef)
        //                    : await _context2.MsSArticle.AnyAsync(a => a.ArRef == cmd.article.ArRef);

        //                if (!exists)
        //                {
        //                    result.Add(new
        //                    {
        //                        article = cmd.article.ArRef,
        //                        status = "❌ Article introuvable",
        //                        action = "ignored"
        //                    });

        //                    continue;
        //                }


        //                // Normaliser la date (date only)
        //                var dateOnly = cmd.Datecreated.Date;

        //                // 🔍 Rechercher s'il existe déjà une commande identique
        //                var existing = await _context.Commandes.FirstOrDefaultAsync(c =>
        //                    c.PharmacyId == cmd.pharmacy.CtNum &&
        //                    c.ArticleId == cmd.article.ArRef &&
        //                    c.CreatedBy == commercial &&
        //                    c.DateCreated.Date == dateOnly
        //                );

        //                if (existing != null)
        //                {
        //                    // 🔹 Cas 1 : exact match → ignorer
        //                    if (existing.Quantity == cmd.Quantity &&
        //                        existing.QuantityVendue == cmd.QuantityVendue)
        //                    {
        //                        result.Add(new
        //                        {
        //                            article = cmd.article.ArRef,
        //                            status = "❌ Cet article est déjà commandé",
        //                            action = "ignored"
        //                        });

        //                        continue;
        //                    }

        //                    // 🔹 Cas 2 : mismatch → mise à jour
        //                    bool updated = false;

        //                    if (existing.Quantity != cmd.Quantity)
        //                    {
        //                        existing.Quantity = cmd.Quantity;
        //                        updated = true;
        //                    }

        //                    if (existing.QuantityVendue != cmd.QuantityVendue)
        //                    {
        //                        existing.QuantityVendue = cmd.QuantityVendue;
        //                        updated = true;
        //                    }

        //                    if (updated)
        //                    {
        //                        existing.LastUpdated = DateTime.UtcNow;

        //                        result.Add(new
        //                        {
        //                            article = cmd.article.ArRef,
        //                            status = "⚠️ Quantité mise à jour",
        //                            action = "updated"
        //                        });
        //                    }

        //                    continue;
        //                }


        //                // 🔹 Cas 3 : nouvelle commande → insertion
        //                var newCmd = new Commande
        //                {
        //                    PharmacyId = cmd.pharmacy.CtNum,
        //                    ArticleId = cmd.article.ArRef,
        //                    Quantity = cmd.Quantity,
        //                    QuantityVendue = cmd.QuantityVendue,
        //                    DateCreated = cmd.Datecreated,
        //                    LastUpdated = DateTime.UtcNow,
        //                    CreatedBy = commercial,
        //                    Source = source,
        //                    DatePeremtion = cmd.DatePeremtion,
        //                    NumSerie = cmd.NumSerie,
        //                    DatePeremtion1 = cmd.DatePeremtion1,
        //                    NumSerie1 = cmd.NumSerie1,
        //                    DatePeremtion2 = cmd.DatePeremtion2,
        //                    NumSerie2 = cmd.NumSerie2,
        //                    Qte1 = cmd.Qte1,
        //                    Qte2 = cmd.Qte2,
        //                    Qte3 = cmd.Qte3,
        //                };

        //                _context.Commandes.Add(newCmd);

        //                result.Add(new
        //                {
        //                    article = cmd.article.ArRef,
        //                    status = "✅ Inséré",
        //                    action = "inserted"
        //                });
        //            }

        //            await _context.SaveChangesAsync();

        //            return Ok(new
        //            {
        //                message = "🚀 Traitement terminé",
        //                details = result
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            return StatusCode(500, $"❌ Erreur : {ex.Message}");
        //        }
        //    }




        [HttpPut("{id}/update-quantity")]
        public async Task<IActionResult> UpdateQuantityVendue(int id, int newQuantityVendue, decimal prixVente)
        {
            var commande = await _context.Commandes.FindAsync(id);

            if (commande == null)
            {
                return NotFound();
            }

            // Mettre à jour la quantité vendue
            commande.QuantityVendue = newQuantityVendue;
            commande.PrixVente = prixVente;
            // Mettre à jour la date de dernière mise à jour
            commande.LastUpdated = DateTime.UtcNow;

            _context.Commandes.Update(commande);
            await _context.SaveChangesAsync();

            return Ok(commande);
        }
        [HttpPut("{id}/update-quantity-image")]
        public async Task<IActionResult> UpdateQuantityVendue(int id, [FromForm] FormDataViewModel formData)
        {
            try
            {
                var commande = await _context.Commandes.FindAsync(id);

                if (commande == null)
                {
                    return NotFound("Commande introuvable !");
                }

                if (formData.file != null && formData.file.Length > 0)
                {
                    // Créer un chemin de fichier unique dans le dossier de téléchargement
                    string uploadsFolder = Path.Combine("C:\\Users\\Administrateur\\Desktop\\Site Novopharma\\Novopharma-Server\\WebApiTest\\wwwroot\\uploads\\");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Utilisez l'ID de la commande dans le nom de fichier pour le rendre unique
                    string uniqueFileName = $"{id}_{formData.file.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Enregistrer le fichier sur le serveur
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formData.file.CopyToAsync(stream);
                    }

                    commande.ImagePath = filePath;
                }



                await _context.SaveChangesAsync();

                return Ok(commande);
            }
            catch (Exception ex)
            {
                // Logguer l'exception pour le débogage
                Console.WriteLine($"Une erreur s'est produite : {ex.Message}");
                return StatusCode(500, "Une erreur interne s'est produite.");
            }
        }

        public class FormDataViewModel
        {
            /*   public int NewQuantityVendue { get; set; }
               public decimal? PrixVente { get; set; }*/
            public IFormFile file { get; set; } // IFormFile pour représenter le fichier dans FormData
        }
        [HttpGet("get-image")]
        public IActionResult GetImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return BadRequest("Le chemin de l'image n'est pas spécifié.");
            }

            // Construire le chemin complet de l'image
            string uploadsFolder = "";
            string imagePathOnServer = Path.Combine(uploadsFolder, imagePath);

            // Vérifier si le fichier existe
            if (!System.IO.File.Exists(imagePathOnServer))
            {
                return NotFound("L'image demandée n'existe pas.");
            }

            // Lire le contenu du fichier
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePathOnServer);

            // Obtenir le type de contenu MIME de l'image (en utilisant une bibliothèque tierce pour obtenir l'extension)
            string contentType = GetContentType(imagePathOnServer);

            // Retourner l'image comme réponse avec le bon type de contenu
            return File(imageBytes, contentType);
        }

        [HttpPost("upload/{id}")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Generate unique file name using command ID
            var fileName = $"{id}{Path.GetExtension(file.FileName)}";

            // Path to the uploads folder
            var uploadsFolder = Path.Combine("wwwroot", "uploads");

            // Ensure the uploads folder exists, if not, create it
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Full path to save the file
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Copy file to server
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Ok(fileName);
        }

        [HttpGet("commande/image/{id}")]
        public IActionResult GetCommandeImage(int id)
        {
            var commande = _context.Commandes.Find(id);

            if (commande == null || string.IsNullOrEmpty(commande.ImagePath))
            {
                return NotFound();
            }

            // Chemin complet vers l'image
            string imagePath = commande.ImagePath;

            // Vérifiez si le fichier existe
            if (!System.IO.File.Exists(imagePath))
            {
                return NotFound();
            }

            // Lire les données de l'image
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);

            // Déterminer le type de contenu de l'image
            string contentType = "image/jpeg"; // Vous pouvez ajuster le type de contenu en fonction de votre type d'image

            // Retourner l'image comme réponse HTTP avec le type de contenu approprié
            return File(imageBytes, contentType);
        }

        private string GetContentType(string fileExtension)
        {
            // Convert the file extension to lower case
            fileExtension = fileExtension.ToLowerInvariant();

            // Determine and return the content type based on the file extension
            switch (fileExtension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                default:
                    return "application/octet-stream";
            }
        }
    }
}


public class CheckInRequest
{
    public string Username { get; set; }
    public string LocalisationCheckIn { get; set; }
    public DateTime DateCheckIn { get; set; }
}

public class CheckOutRequest
{
    public string Username { get; set; }
    public string LocalisationCheckOut { get; set; }
    public DateTime DateCheckOut { get; set; }
}
