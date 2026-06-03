namespace WebApiTest.Interface
{
	public interface IAuthRepository
	{
		bool Login(string username, string password);

	}
}
