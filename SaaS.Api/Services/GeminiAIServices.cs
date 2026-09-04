using Google.GenAI;
using SaaS.Api.Services.Interfaces;
using System.Threading.Tasks;

namespace SaaS.Api.Services
{
    public class GeminiAIServices : IAIServices
    {
        private readonly Client _client;
        public GeminiAIServices(IConfiguration configuration)
        {
            var apikey = configuration["GEMINI_API_KEY"];
            _client = new Client(apiKey: apikey);
        }
        public async Task Test(string content) {
            //Khoi tao client 
       
            var response = await _client.Models.GenerateContentAsync(
                model: "gemini-3.5-flash",
                contents: content
            );
            Console.WriteLine(response.Candidates[0].Content.Parts[0]?.Text);
        }
    }
}
