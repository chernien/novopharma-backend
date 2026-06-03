using WebApiTest.Data;
using WebApiTest.Interface;
using WebApiTest.Models;

namespace WebApiTest.Repository
{
	public class MsEnttRepository : IMsEnttRepository
	{
		private readonly NOVOPHARMAContext _context;
		public MsEnttRepository(NOVOPHARMAContext context)
		{
			_context = context;
		}
		//Get Factures
		public ICollection<MsAEntt> GetMsAEnttsFacture(string code_client)
		{
			return _context.MsAEntt.Where(e => (e.DoType == 7 || e.DoType == 6) && e.DoTiers == code_client).ToList();
		}
		//Get Commandes
		public ICollection<MsAEntt> GetMsAEnttsCommande(string code_client)
		{
			return _context.MsAEntt.Where(e => e.DoType == 1 && e.DoTiers == code_client).ToList();
		}

		public ICollection<MsALigne> GetLignesByDop(string dop)
		{
			return _context.MsALigne.Where(l => l.DoPiece == dop).ToList();
		}
		public ICollection<MsAEntt> GetMsAEnttsCommandeMois(string code_client)
		{
			var data = _context.MsAEntt.ToList();
			var filteredData = data.Where(e => CalculDiff(e.DoDate.ToString()) <= 31 && e.DoTiers == code_client).ToList();
			return filteredData;
		}
		public ICollection<MsAEntt> GetMsAEnttsCommandeTrimestre(string code_client)
		{
			var data = _context.MsAEntt.ToList();
			var filteredData = data.Where(e => CalculDiff(e.DoDate.ToString()) <= 93 && e.DoTiers == code_client).ToList();
			return filteredData;
		}
		int CalculDiff(string? date_commande)
		{
		DateTime date_debut = DateTime.Parse(date_commande);
			DateTime fin = DateTime.Now;
			TimeSpan? Diff = fin - date_debut;
			int Diff_date = (int)Diff?.TotalDays;
			return Diff_date;
		}
	}
}
