using WebApiTest.Models;

namespace WebApiTest.Interface
{
	public interface IMs_ContactRepository
	{
		ICollection<Ms_Contactsite> GetContactsites();
		public bool CreateContact(Ms_Contactsite contact);
		Ms_Contactsite GetMs_Contactsite(int id);
		public bool DeleteContactSite(Ms_Contactsite ms_Contactsite);
	}
}
