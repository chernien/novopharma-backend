using WebApiTest.Data;
using WebApiTest.Interface;
using WebApiTest.Models;

namespace WebApiTest.Repository
{
	public class Fa_FamilleRepository : IFa_FamilleRepository
	{
		private readonly NOVOPHARMAContext _context;
		public Fa_FamilleRepository(NOVOPHARMAContext context)
		{

			_context = context;
		}

		public F_Famille GetF_Famille(string id_famille)
		{
			return _context.F_Famille.Where(f=>f.FA_CodeFamille==id_famille).FirstOrDefault();
		}
	}
}
