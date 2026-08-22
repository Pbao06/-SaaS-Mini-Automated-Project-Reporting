namespace SaaS.Api.Services.Interfaces
{
    public interface IAuthServices
    {
        public void Register();
        public void Logout();
        public void Login();
    }
}
