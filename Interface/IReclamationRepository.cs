using WebApiTest.Models;

namespace WebApiTest.Interface
{
	public interface IReclamationRepository
	{
		ICollection<MsReclamationweb> GetReclamationwebs();
		public bool CreateReclamation(MsReclamationweb reclamation, string reclamation_type, string client);
		MsReclamationweb GetReclamationweb(int id);
		public bool DeleteRecalamation(MsReclamationweb msReclamationweb);
	}
}
