using Microsoft.EntityFrameworkCore;
using WebApiTest.Models;

namespace WebApiTest.Interface
{
	public interface IClientRepository
	{
		bool Login(string username, string password);
		MsAClients GetClientByNum(string code);
		ICollection<MsAClients> GetClients();
		ICollection<LoginRequest> GetClientsAuth();

		LoginRequest GetClientAuth(int id);
		LoginRequest GetClientAuthuser(string username);
		bool UpdateUserAuth(LoginRequest user);
        bool UpdateUserCheckIn(string username, string localisationCheckIn, DateTime dateCheckIn);
        bool UpdateUserCheckOut(string username, string localisationCheckOut, DateTime dateCheckOut);


    }

}
