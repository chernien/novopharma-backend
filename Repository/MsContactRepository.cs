using Microsoft.IdentityModel.Tokens;
using WebApiTest.Data2;
using WebApiTest.Interface;
using WebApiTest.Models;

namespace WebApiTest.Repository
{
	public class MsContactRepository : IMs_ContactRepository
	{
		private readonly PowerAppContext _context;

		public MsContactRepository(PowerAppContext context)
		{
			_context = context;
		}
		public bool CreateContact(Ms_Contactsite contact)
		{
			_context.Ms_Contactsite.Add(contact);
			return Save();
		}

		public bool DeleteContactSite(Ms_Contactsite ms_Contactsite)
		{
			_context.Remove(ms_Contactsite);
			return Save();
		}

		public ICollection<Ms_Contactsite> GetContactsites()
		{
			return _context.Ms_Contactsite.ToList();
		}

		public Ms_Contactsite GetMs_Contactsite(int id)
		{
			return _context.Ms_Contactsite.Where(c => c.Id == id).FirstOrDefault();
		}
		public bool Save()
		{
			var saved = _context.SaveChanges();
			return saved > 0 ? true : false;
		}

	}
}
