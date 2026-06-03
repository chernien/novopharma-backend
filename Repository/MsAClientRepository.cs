using WebApiTest.Data;
using WebApiTest.Data2;
using WebApiTest.Interface;
using WebApiTest.Models;

namespace WebApiTest.Repository
{
	public class MsAClientRepository : IClientRepository
	{
		private readonly NOVOPHARMAContext _context;
		private readonly MEDSOURCEContext _context2;

		private readonly PowerAppContext _contextPowerApps;
		public MsAClientRepository(NOVOPHARMAContext context, PowerAppContext contextPowerApps)
		{
			_context = context;
			_context2 = _context2;

		}

		public MsAClients GetClientByNum(string code)
		{
			return _context.MsAClients.Where(c => c.CtNum == code).FirstOrDefault();
		}

        public ICollection<MsAClients> GetClients()
        {
            var clients1 = new List<MsAClients>();
            var clients2 = new List<MsAClients>();

            try
            {
                // 🔹 Base NOVOPHARMA
                clients1 = _context.MsAClients.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors du chargement des clients NOVOPHARMA : " + ex.Message);
            }

            try
            {
                // 🔹 Base MEDSOURCE
                if (_context2 != null)
                    clients2 = _context2.MsAClients.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors du chargement des clients MEDSOURCE : " + ex.Message);
            }

            // 🔹 Fusion + suppression des doublons selon CtNum
            var allClients = clients1
                .Concat(clients2)
                .GroupBy(c => c.CtNum)
                .Select(g => g.First())
                .OrderBy(c => c.CtIntitule)
                .ToList();

            return allClients;
        }

        public ICollection<LoginRequest> GetClientsAuth()
		{
			return _context.Authentication.OrderBy(c => c.id).ToList();
		}
		public LoginRequest GetClientAuth(int id)
		{
			return _context.Authentication.Where(c => c.id == id).FirstOrDefault();
		}

		public bool Login(string username, string password)
		{
			return _context.Authentication.Any(c => c.username.Trim() == username.Trim() && c.password.Trim() == password.Trim() && c.enabled==true);
		
		}

        

        public bool UpdateUserAuth(LoginRequest user)
		{
			_context.Update(user);
			return Save();
		}

        public bool UpdateUserCheckIn(string username, string localisationCheckIn, DateTime dateCheckIn)
        {
            var user = _context.Authentication.FirstOrDefault(u => u.username == username);
            if (user == null) return false;

            user.LocalisationCheckIn = localisationCheckIn;
            user.DateCheckIn = dateCheckIn;

            _context.SaveChanges();
            return true;
        }

        public bool UpdateUserCheckOut(string username, string localisationCheckOut, DateTime dateCheckOut)
        {
            var user = _context.Authentication.FirstOrDefault(u => u.username == username);
            if (user == null) return false;

            user.LocalisationCheckOut = localisationCheckOut;
            user.DateCheckOut = dateCheckOut;

            _context.SaveChanges();
            return true;
        }



        public bool Save()
		{
			var saved = _context.SaveChanges();
			return saved > 0 ? true : false;
		}

		public LoginRequest GetClientAuthuser(string username)
		{
			return _context.Authentication.Where(c => c.username == username).FirstOrDefault();
		}

	}
}
