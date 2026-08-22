using Google.GenAI;
using SaaS.Api.Services.Interfaces;
using System.Threading.Tasks;

namespace SaaS.Api.Services
{
    public class AIServices : IAIServices
    {
        public async Task Test(string content) {
            //Khoi tao client 
            var key = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            var client = new Google.GenAI.Client();
            var response = await client.Models.GenerateContentAsync(
                model: "gemini-3.5-flash",
                contents: content
                );
            Console.WriteLine(response.Candidates[0].Content.Parts[0].Text);
        }
    }
}
