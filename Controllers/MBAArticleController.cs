using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiTest.Data;
using WebApiTest.Interface;
using WebApiTest.Models;

namespace WebApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MBAArticleController : ControllerBase
    {
        private readonly IMbArticleRepository _mbArticleRepository;
        private readonly IFa_FamilleRepository _FamilleRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly NOVOPHARMAContext _context;
        private readonly MEDSOURCEContext _context2;

        public MBAArticleController(NOVOPHARMAContext context, MEDSOURCEContext context2, IMbArticleRepository mbArticleRepository, IFa_FamilleRepository fa_FamilleRepository, IWebHostEnvironment environment)
        {
            _mbArticleRepository = mbArticleRepository;
            _FamilleRepository = fa_FamilleRepository;
            _environment = environment;
            _context = context;
            _context2 = context2;
        }
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(ICollection<MBAArticle>))]
        public IActionResult GetAllArticles()
        {
            var articles = _context.MBAArticle.AsNoTracking()
        .Where(m => !(m.FaCodeFamille.StartsWith("05") ||
                      m.FaCodeFamille.StartsWith("06") ||
                      m.AR_Sommeil == 1 ||
                      m.FaCodeFamille == "TFV" ||
                      m.FaCodeFamille == "TFA"))
        .GroupBy(a => a.ArRef)
        .Select(g => g.First())
        .ToList(); ;

            return Ok(articles);
        }
        [HttpGet("all-marques")]
        [ProducesResponseType(200, Type = typeof(List<string>))]
        public IActionResult GetAllMarques()
        {
            var marquesNovopharma = _context.MBAArticle
                .AsNoTracking()
                .Where(m => !(m.FaCodeFamille.StartsWith("05") ||
                              m.FaCodeFamille.StartsWith("06") ||
                              m.AR_Sommeil == 1 ||
                              m.FaCodeFamille == "TFV" ||
                              m.FaCodeFamille == "TFA"))
                .Select(a => string.IsNullOrEmpty(a.Marque.Trim()) ? "Med Source" : a.Marque)
                .Distinct()
                .ToList();

            var marquesMedsource = _context2.MBAArticle
                .AsNoTracking()
                .Where(m => !(m.FaCodeFamille.StartsWith("05") ||
                              m.FaCodeFamille.StartsWith("06") ||
                              m.FaCodeFamille == "TFV" ||
                              m.FaCodeFamille == "TFA"))
                .Select(a => string.IsNullOrEmpty(a.Marque.Trim()) ? "Med Source" : a.Marque)
                .Distinct()
                .ToList();

            // Fusionner et supprimer les doublons
            var allMarques = marquesNovopharma.Concat(marquesMedsource)
                                               .Distinct()
                                               .ToList();

            return Ok(allMarques);
        }

        [HttpGet("articles-par-marque/{marque?}")]
        [ProducesResponseType(200, Type = typeof(List<object>))]
        public IActionResult GetArticlesByMarque(string? marque)
        {
            // Si la marque est vide ou null, on traite les articles ayant une marque null ou vide
            if (string.IsNullOrEmpty(marque))
            {
                // Récupérer les articles de Novopharma avec une marque vide ou null
                var articlesNovopharma = _context.MBAArticle
                    .AsNoTracking()
                    .Where(m => (!(m.FaCodeFamille.StartsWith("05") ||
                              m.FaCodeFamille.StartsWith("06") ||
                              m.FaCodeFamille == "TFV" ||
                              m.FaCodeFamille == "TFA")) && string.IsNullOrEmpty(m.Marque))
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
                        a.AR_CodeBarre,
                        Source = "Novopharma"
                    })
                    .ToList();

                // Récupérer les articles de Medsource avec une marque vide ou null
                var articlesMedsource = _context2.MBAArticle
                    .AsNoTracking()
                              .Where(m => (!(m.FaCodeFamille.StartsWith("05") ||
                              m.FaCodeFamille.StartsWith("06") ||
                              m.FaCodeFamille == "TFV" ||
                              m.FaCodeFamille == "TFA")) && string.IsNullOrEmpty(m.Marque)).Select(a => new
                              {
                                  a.ArRef,
                                  a.ArDesign,
                                  a.Marque,
                                  a.FaCodeFamille,
                                  a.Ar_photo,
                                  a.Famille,
                                  a.AR_Sommeil,
                                  a.Recommande,
                                  a.AR_CodeBarre,
                                  Source = "Medsource"
                              })
                    .ToList();

                // Fusionner les deux listes d'articles
                var allArticles = articlesNovopharma.Concat(articlesMedsource).ToList();

                if (allArticles.Count == 0)
                {
                    return NotFound("Aucun article trouvé avec une marque vide ou null.");
                }

                return Ok(allArticles);
            }
            else
            {
                // Sinon, récupérer les articles correspondant à la marque spécifique
                marque = marque.Trim();

                // Récupérer les articles de Novopharma (sans appliquer Trim ou Equals directement en DB)
                var articlesNovopharma = _context.MBAArticle
                    .AsNoTracking()
                    .Where(m => m.Marque == marque)
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
                        a.AR_CodeBarre,
                        Source = "Novopharma"
                    })
                    .ToList();

                // Récupérer les articles de Medsource (sans appliquer Trim ou Equals directement en DB)
                var articlesMedsource = _context2.MBAArticle
                    .AsNoTracking()
                    .Where(m => m.Marque == marque)
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
                        a.AR_CodeBarre,
                        Source = "Medsource"
                    })
                    .ToList();

                // Appliquer le Trim et comparer les marques en mémoire (côté serveur)
                var allArticles = articlesNovopharma.Concat(articlesMedsource)
                    .Where(a => a.Marque.Trim().Equals(marque, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (allArticles.Count == 0)
                {
                    return NotFound("Aucun article trouvé pour cette marque.");
                }

                return Ok(allArticles);
            }
        }

        [HttpGet("all-articles")]
        [ProducesResponseType(200, Type = typeof(ICollection<object>))]
        public IActionResult GetAllArticlesCombined()
        {
            // 🔹 Récupérer les articles de Novopharma avec Source
            var articlesNovopharma = _context.MBAArticle.AsNoTracking()
                .Where(m => !(m.FaCodeFamille.StartsWith("05") ||
                              m.FaCodeFamille.StartsWith("06") ||
                              m.AR_Sommeil == 1 ||
                              m.FaCodeFamille == "TFV" ||
                              m.FaCodeFamille == "TFA"))
                .GroupBy(a => a.ArRef)
                .Select(g => new
                {
                    ArRef = g.First().ArRef,
                    ArDesign = g.First().ArDesign,
                    Marque = g.First().Marque,
                    FaCodeFamille = g.First().FaCodeFamille,
                    Ar_photo = g.First().Ar_photo,
                    Famille = g.First().Famille,
                    AR_Sommeil = g.First().AR_Sommeil,
                    Recommande = g.First().Recommande,
                    AR_CodeBarre = g.First().AR_CodeBarre,
                    Source = "Novopharma" // ✅ Ajout de la source
                })
                .ToList();

            // 🔹 Récupérer les articles de Medsource avec Source
            var articlesMedsource = _context2.MBAArticle.AsNoTracking()
                .Where(m => !(m.FaCodeFamille.StartsWith("05") ||
                              m.FaCodeFamille.StartsWith("06") ||
                              m.FaCodeFamille == "TFV" ||
                              m.FaCodeFamille == "TFA"))
                .GroupBy(a => a.ArRef)
                .Select(g => new
                {
                    ArRef = g.First().ArRef,
                    ArDesign = g.First().ArDesign,
                    Marque = g.First().Marque,
                    FaCodeFamille = g.First().FaCodeFamille,
                    Ar_photo = g.First().Ar_photo,
                    Famille = g.First().Famille,
                    AR_Sommeil = g.First().AR_Sommeil,
                    Recommande = g.First().Recommande,
                    AR_CodeBarre = g.First() .AR_CodeBarre,
                    Source = "Medsource" // ✅ Ajout de la source
                })
                .ToList();

            // 🔹 Fusionner les deux listes (Maintenant elles ont la même structure)
            var allArticles = articlesNovopharma.Concat(articlesMedsource).ToList();

            return Ok(allArticles);
        }


        //ARTICLE MED SOURCE
        [HttpGet("medsource")]
        [ProducesResponseType(200, Type = typeof(ICollection<MsAArticle>))]
        public IActionResult GetAllArticlesMS()
        {
            var articles = _context2.MbArticle.AsNoTracking()
        .Where(m => !(m.FaCodeFamille.StartsWith("05") ||
                      m.FaCodeFamille.StartsWith("06") ||
                      m.FaCodeFamille == "TFV" ||
                      m.FaCodeFamille == "TFA"))
        .GroupBy(a => a.ArRef) // Grouper par 'ArRef' pour obtenir l'unicité
        .Select(g => g.First()) // Sélectionner le premier élément de chaque groupe
        .ToList(); ;

            return Ok(articles);
        }
        //ARTICLE MED SOURCE
        [HttpGet("medsource/{id}")]
        [ProducesResponseType(200, Type = typeof(MsAArticle))]
        public IActionResult GetAllArticlesMSById(string id)
        {
            var articles = _context2.MsSArticle.AsNoTracking()
        .Where(m => !(m.FaCodeFamille.StartsWith("05") ||
                      m.FaCodeFamille.StartsWith("06") ||
                      m.FaCodeFamille == "TFV" ||
                      m.FaCodeFamille == "TFA") && m.ArRef == id)
        // Grouper par 'ArRef' pour obtenir l'unicité
        .FirstOrDefault();

            return Ok(articles);
        }
        [HttpGet("gift")]
        [ProducesResponseType(200, Type = typeof(ICollection<MsAArticle>))]
        public IActionResult GetAllArticlesGifts()
        {
            var articles = _context.MBAArticle.AsNoTracking().Where(m => m.FaCodeFamille.StartsWith("05"))
        .GroupBy(a => a.ArRef) // Grouper par 'ArRef' pour obtenir l'unicité
        .Select(g => g.First()) // Sélectionner le premier élément de chaque groupe
        .ToList(); ;

            return Ok(articles);
        }

        [HttpGet("avance")]
        [ProducesResponseType(200, Type = typeof(ICollection<MsAArticle>))]
        public IActionResult GetArticlesPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 70)
        {
            var articles = _context.MsAArticle.AsNoTracking()
                .Where(m => !(m.FaCodeFamille.StartsWith("05") ||
                              m.FaCodeFamille.StartsWith("06") ||
                              m.AR_Sommeil == 1 ||
                              m.FaCodeFamille == "TFV" ||
                              m.FaCodeFamille == "TFA"))
                .GroupBy(a => a.ArRef)
                .Select(g => g.First());

            // Pagination
            var pagedArticles = articles
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(pagedArticles);
        }

        [HttpGet("promos")]
        [ProducesResponseType(200, Type = typeof(ICollection<MsAArticle>))]
        public IActionResult GetAllArticlesPromo()
        {
            var articles = _mbArticleRepository.GetMbArticlesPromo();

            return Ok(articles);
        }


        [HttpGet("recommande")]
        [ProducesResponseType(200, Type = typeof(ICollection<MsAArticle>))]
        public IActionResult GetAllArticlesRec()
        {
            var articles = _context.MBAArticle.AsNoTracking().Where(a => a.Recommande.Trim() == "Oui");

            return Ok(articles);
        }

        /*	[HttpPost("UploadImage")]
			public async Task<ActionResult> UploadImage()
			{
				bool Results = false;
				try
				{
					var _uploadedfiles = Request.Form.Files;
					foreach (IFormFile source in _uploadedfiles)
					{
						string Filename = source.FileName;
						string Filepath = GetFilePath(Filename);

						if (!System.IO.Directory.Exists(Filepath))
						{
							System.IO.Directory.CreateDirectory(Filepath);
						}

						string imagepath = Filepath + "\\image.png";

						if (System.IO.File.Exists(imagepath))
						{
							System.IO.File.Delete(imagepath);
						}
						using (FileStream stream = System.IO.File.Create(imagepath))
						{
							await source.CopyToAsync(stream);
							Results = true;
						}

					}
				}
				catch (Exception ex)
				{

				}
				return Ok(Results);
			}
	*/
        /*[NonAction]
		private File GetFilePath(string code_article)
		{
			var filePath = this._environment.WebRootPath + "\\Uploads\\Articles\\" + code_article;

			return System.Web.
			
		}*/

        [HttpGet("{arRef}")]
        [ProducesResponseType(200, Type = typeof(MsAArticle))]
        [ProducesResponseType(400)]
        public IActionResult GetArticleByRef(string arRef)
        {
            var article = _mbArticleRepository.GetMbArticle(arRef);
            if (!_mbArticleRepository.ArticleExist(arRef))
            {
                return NotFound("article with this id is not found !");
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(article);
        }

        [HttpPut("recommande/{arRef}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateArticleRec(string arRef)
        {
            var currentDetail = _context.F_Article.FirstOrDefault(a => a.ArRef.Trim() == arRef.Trim());

            if (currentDetail == null)
            {
                return NotFound();
            }

            // Basculer la valeur du champ "Recommandé"
            if (string.IsNullOrWhiteSpace(currentDetail.Recommande))
            {
                currentDetail.Recommande = "Oui"; // Si le champ est vide ou null, le mettre à "oui"
            }
            else
            {
                currentDetail.Recommande = string.Empty;
            }

            // Désactiver temporairement les déclencheurs sur la table 'F_ARTICLE'
            _context.Database.ExecuteSqlRaw("DISABLE TRIGGER ALL ON F_ARTICLE");

            _context.Update(currentDetail); // Signaler que l'entité a été modifiée

            await _context.SaveChangesAsync(); //

            // Réactiver les déclencheurs sur la table 'F_ARTICLE'
            _context.Database.ExecuteSqlRaw("ENABLE TRIGGER ALL ON F_ARTICLE");

            return Ok(currentDetail);
        }
        [HttpPut("recommande/medsource/{arRef}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateArticleRecMed(string arRef)
        {
            var currentDetail = _context2.F_Article.FirstOrDefault(a => a.ArRef.Trim() == arRef.Trim());

            if (currentDetail == null)
            {
                return NotFound();
            }

            // Basculer la valeur du champ "Recommandé"
            if (string.IsNullOrWhiteSpace(currentDetail.Recommande))
            {
                currentDetail.Recommande = "Oui"; // Si le champ est vide ou null, le mettre à "oui"
            }
            else
            {
                currentDetail.Recommande = string.Empty;
            }

            // Désactiver temporairement les déclencheurs sur la table 'F_ARTICLE'
            _context2.Database.ExecuteSqlRaw("DISABLE TRIGGER ALL ON F_ARTICLE");

            _context2.Update(currentDetail); // Signaler que l'entité a été modifiée

            await _context2.SaveChangesAsync(); //

            // Réactiver les déclencheurs sur la table 'F_ARTICLE'
            _context2.Database.ExecuteSqlRaw("ENABLE TRIGGER ALL ON F_ARTICLE");

            return Ok(currentDetail);
        }

        [HttpGet("image/{article_code}")]
        public async Task<IActionResult> GetFileById(string article_code)

        {
            var article = this._mbArticleRepository.GetMbArticle(article_code);
            var path = "C:\\Users\\Public\\Documents\\Sage\\Entreprise 100c\\" + article.Ar_Photo;


            if (System.IO.File.Exists(path))
            {
                return File(System.IO.File.OpenRead(path), "application/octet-stream", Path.GetFileName(path));
            }
            return NotFound();
        }




        [HttpGet("famille/{id_famille}")]
        public async Task<IActionResult> GetFamilleArticle(string id_famille)

        {
            var famille = _FamilleRepository.GetF_Famille(id_famille).FA_Intitule;
            return Ok(famille);
        }
        [HttpPut("detail/{arRef}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdateArticle(string arRef, [FromBody] DetailArticle updateArticle)
        {
            var currentDetail = _mbArticleRepository.GetDetailMbArticle(arRef);

            if (currentDetail == null)
            {
                return NotFound();
            }
            currentDetail.Description = updateArticle.Description;
            currentDetail.C_Utilisation = updateArticle.C_Utilisation;
            currentDetail.Composition = updateArticle.Composition;


            if (!ModelState.IsValid)
                return BadRequest();

            _mbArticleRepository.UpdateArticle(currentDetail, arRef);
            return Ok(currentDetail);


        }
        [HttpPost("detail/{arRef}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public IActionResult CreateDetails([FromBody] DetailArticle detail, string arRef)
        {
            if (detail == null)
                return BadRequest(ModelState);

            if (!_mbArticleRepository.addDetailArticle(detail, arRef))
            {
                ModelState.AddModelError("", "Something went wrong while savin");
                return StatusCode(500, ModelState);
            }
            /*			detail.arRef = arRef;
            */
            return Ok(detail);
        }
    }

}
