using Microsoft.EntityFrameworkCore;
using WebApiTest.Models;

namespace WebApiTest.Interface
{
	public interface IMbArticleRepository
	{
		ICollection<MsAArticle> GetMbArticles();
        ArticleDto GetMbArticle(string id);
		bool ArticleExist(string id);
		bool ArticledetailExist(int id);
		ICollection<MsAArticle> GetMbArticlesPromo();
		public void UpdateArticle(DetailArticle detail,string arRef);
		bool addDetailArticle(DetailArticle detail,string arRef);
		DetailArticle GetDetailMbArticle(string id);


	}
}
