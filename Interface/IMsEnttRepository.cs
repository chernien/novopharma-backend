using WebApiTest.Models;

namespace WebApiTest.Interface
{
	public interface IMsEnttRepository
	{
		ICollection<MsAEntt> GetMsAEnttsFacture(string code_client);
		ICollection<MsAEntt> GetMsAEnttsCommande(string code_client);
		ICollection<MsALigne> GetLignesByDop(string dop);
		ICollection<MsAEntt> GetMsAEnttsCommandeMois(string code_client);
		ICollection<MsAEntt> GetMsAEnttsCommandeTrimestre(string code_client);
	}
}
