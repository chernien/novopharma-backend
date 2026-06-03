using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using WebApiTest.Data;
using WebApiTest.Models;

namespace WebApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MB_ARTController : ControllerBase
    {
        private readonly NOVOPHARMAContext _context;

        public MB_ARTController(NOVOPHARMAContext context)
        {
            _context = context;
        }

        // GET: api/MB_ART
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MB_ART>>> GetMB_ARTS()
        {
            // On peut ajouter un filtre si nécessaire, mais sinon on récupère tous les articles.
            var articles = await _context.MB_ARTS
                .AsNoTracking()  
                .ToListAsync();

            return Ok(articles);
        }
        [HttpGet("search/{query}")]
        public async Task<ActionResult<MB_ART>> GetMB_ARTByRefOrNoSerie(string query)
        {
            // Validation de l'entrée
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("La recherche ne peut pas être vide.");
            }

            try
            {
                // Recherche optimisée avec AsNoTracking
                var mb_art = await _context.MB_ARTS
                    .AsNoTracking() // Désactive le suivi des entités pour une lecture plus rapide
                    .FirstOrDefaultAsync(a => a.AR_Ref == query || a.LS_NoSerie == query);

                if (mb_art == null)
                {
                    return NotFound($"Aucun article trouvé pour la recherche '{query}'.");
                }

                return Ok(mb_art);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur du serveur : {ex.Message}");
            }
        }

        [HttpGet("numlot/{id}")]
        public async Task<ActionResult<MB_ART>> GetMB_ARTparLot(string id)
        {
            var mb_arts = await _context.MB_ARTS.Where(a => a.LS_NoSerie == id).FirstOrDefaultAsync();

            if (mb_arts == null)
            {
                return NotFound();
            }

            return mb_arts;
        }
        [HttpGet("numserie/{nos}/{id}")]
        public async Task<ActionResult<MB_ART>> GetMB_ARTNs(string nos,string id)
        {
            var mb_art = await _context.MB_ARTS.Where(a => a.AR_Ref == id && a.LS_NoSerie ==nos).FirstOrDefaultAsync();

            if (mb_art == null)
            {
                return NotFound();
            }

            return mb_art;
        }
        // GET: api/MB_ART/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MB_ART>> GetMB_ART(string id)
        {
            var mb_art = await _context.MB_ARTS.Where(a => a.AR_Ref == id).FirstOrDefaultAsync();

            if (mb_art == null)
            {
                return NotFound();
            }

            return mb_art;
        }
        // POST: api/MB_ART
        [HttpPost]
        public async Task<ActionResult<MB_ART>> PostMB_ART(MB_ART mb_art)
        {
            _context.MB_ARTS.Add(mb_art);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MB_ARTExists(mb_art.AR_Ref))
                {
                    return Conflict(new { message = "L'article existe déjà avec cette référence." });
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction(nameof(GetMB_ART), new { id = mb_art.AR_Ref }, mb_art);
        }

        // PUT: api/MB_ART/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMB_ART(string id, MB_ART mb_art)
        {
            if (id != mb_art.AR_Ref)
            {
                return BadRequest(new { message = "La référence de l'article ne correspond pas." });
            }

            _context.Entry(mb_art).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MB_ARTExists(id))
                {
                    return NotFound(new { message = "Article non trouvé pour mise à jour." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/MB_ART/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMB_ART(string id)
        {
            var mb_art = await _context.MB_ARTS.FindAsync(id);

            if (mb_art == null)
            {
                return NotFound(new { message = "Article non trouvé pour suppression." });
            }

            _context.MB_ARTS.Remove(mb_art);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MB_ARTExists(string id)
        {
            return _context.MB_ARTS.Any(e => e.AR_Ref == id);
        }
    }
}
