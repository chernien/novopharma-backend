using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebApiTest.Data;
using WebApiTest.Interface;
using WebApiTest.Models;

namespace WebApiTest.Repository
{
    public class MbArticleRepository : IMbArticleRepository
    {
        public readonly NOVOPHARMAContext _context;
        public readonly MEDSOURCEContext _context2;
        public MbArticleRepository(NOVOPHARMAContext nOVOPHARMAContext , MEDSOURCEContext MEDSOURCEContext)
        {
            _context = nOVOPHARMAContext;
            _context2 = MEDSOURCEContext;
        }

        public bool ArticleExist(string id)
        {
            return _context.MsAArticle.Any(a => a.ArRef == id);
        }

        public ArticleDto GetMbArticle(string id)
        {
            // Contexte NOVOPHARMA
            var article1 = _context.MsAArticle.AsNoTracking()
                .FirstOrDefault(a => a.ArRef == id);

            if (article1 != null)
            {
                return new ArticleDto
                {
                    ArRef = article1.ArRef,
                   // ArDesign = article1.ArDesign,
                   // Marque = article1.Marque,
                    Ar_Photo = article1.Ar_photo
                };
            }

            // Contexte MEDSOURCE
            var article2 = _context2.MsSArticle.AsNoTracking()
                .FirstOrDefault(a => a.ArRef == id);

            if (article2 != null)
            {
                return new ArticleDto
                {
                    ArRef = article2.ArRef,
                   // ArDesign = article2.ArDesign,
                    //Marque = article2.Marque,
                    Ar_Photo = article2.Ar_Photo
                };
            }

            return null;
        }



        public ICollection<MsAArticle> GetMbArticles()
        {
            return _context.MsAArticle.AsNoTracking()
        .Where(m => !(m.FaCodeFamille.StartsWith("05") ||
                      m.FaCodeFamille.StartsWith("06") ||
                      m.AR_Sommeil == 1 ||
                      m.FaCodeFamille == "TFV" ||
                      m.FaCodeFamille == "TFA"))
        .GroupBy(a => a.ArRef) // Grouper par 'ArRef' pour obtenir l'unicité
        .Select(g => g.First()) // Sélectionner le premier élément de chaque groupe
        .ToList();
        }
        public ICollection<MsAArticle> GetMbArticlesPromo()
        {
            return _context.MsAArticle.AsNoTracking().Where(m => m.Nouveaute == "Oui").ToList();
        }


        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool addDetailArticle(DetailArticle detail, string arRef)
        {
            detail.arRef = arRef;
            _context.Add(detail);
            return Save();
        }

        public bool ArticledetailExist(int id)
        {
            return _context.DetailArticle.Any(a => a.Id == id);
        }

        public void UpdateArticle(DetailArticle detail, string arRef)
        {
            var existingDetail = _context.DetailArticle.Where(a => a.arRef == arRef).FirstOrDefault();

            if (existingDetail != null)
            {
                existingDetail.Description = detail.Description;
                existingDetail.Composition = detail.Composition;
                existingDetail.C_Utilisation = detail.C_Utilisation;

                _context.SaveChanges();
            }
        }
        public DetailArticle GetDetailMbArticle(string arRef)
        {
            return _context.DetailArticle.Where(a => a.arRef == arRef).FirstOrDefault();
        }
    }
}
