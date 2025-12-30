using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace OpenPlaDiC.DAL.Repositories
{
    public interface IExternalAuthRepository
    {
        Task<bool> VerifyCredentialsAsync(string user, string pass);
    }

    public class ExternalAuthRepository : IExternalAuthRepository
    {
        private readonly HttpClient _httpClient;
        public ExternalAuthRepository(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<bool> VerifyCredentialsAsync(string user, string pass)
        {
            var response = await _httpClient.PostAsJsonAsync("api/login", new { user, pass });
            return response.IsSuccessStatusCode;
        }
    }
}
