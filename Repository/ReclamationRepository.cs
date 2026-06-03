using WebApiTest.Data2;
using WebApiTest.Interface;
using WebApiTest.Models;

namespace WebApiTest.Repository
{
	public class ReclamationRepository : IReclamationRepository
	{
		private readonly PowerAppContext _context;
		public ReclamationRepository(PowerAppContext context)
		{
			_context = context;
		}
		public bool CreateReclamation(MsReclamationweb reclamation, string type_result, string client)
		{
			reclamation.Type = type_result;
			reclamation.Client = client;
			_context.Add(reclamation);
			return Save();
		}
		public bool Save()
		{
			var saved = _context.SaveChanges();
			return saved > 0 ? true : false;
		}


		public ICollection<MsReclamationweb> GetReclamationwebs()
		{
			return _context.MsReclamationweb.ToList();
		}

		public MsReclamationweb GetReclamationweb(int id)
		{
			return _context.MsReclamationweb.Where(r => r.Id == id).FirstOrDefault();
		}

		public bool DeleteRecalamation(MsReclamationweb reclamationweb)
		{
			_context.Remove(reclamationweb);
			return Save();
		}
	}
}
