using sav.Models;

namespace sav.Repository
{
    public interface IClientRepository
    {
        Task<IEnumerable<Client>> GetAllClientsAsync(); 
        Task<Client> GetClientByIdAsync(int clientId);  
        Task AddClientAsync(Client client);
        Task<Client> AuthenticateClientAsync(string email, string password);
        Task<Client> GetClientByEmailAsync(string email);


        Task UpdateClientAsync(Client client);         
        Task DeleteClientAsync(int clientId);          
        Task<bool> SaveChangesAsync();                 
    }
}
