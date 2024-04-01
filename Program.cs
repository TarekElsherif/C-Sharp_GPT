using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GPT_API_Test
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        public static string _apiKey = "API-KEY"; // TODO: insert your OpenAI API key here
        public static string _endpointURL = "https://api.openai.com/v1/chat/completions";
        public static string _modelType = "gpt-3.5-turbo";
        public static int _maxTokens = 256;
        public static double _temprature = 0.1;

        public struct Message
        {
            [JsonProperty("role")]
            public string Role;
            [JsonProperty("content")]
            public string Content;
        }

        public struct Choices
        {
            [JsonProperty("message")]
            public Message Message;
        }

        public struct Response
        {
            [JsonProperty("choices")]
            public Choices[] Choices;
        }

        public static async Task Main(string[] args)
        {
            await StartChat();
        }

        public static async Task StartChat()
        {
            Console.WriteLine("Type something and press enter:\n");
            string input = Console.ReadLine();
            Console.WriteLine("\n[Receiving Response...]\n");
            string response = await OpenAISendPrompt(input);
            Console.WriteLine(response + "\n");
            await StartChat();
        }

        public static async Task<string> OpenAISendPrompt(string prompt)
        {
            Message[] prompts = new Message[1] { new Message { Role = "user", Content = prompt } };
            string stringResponse =
                await OpenAIComplete(_apiKey, _endpointURL, _modelType, prompts, _maxTokens, _temprature);
            Response response = JsonConvert.DeserializeObject<Response>(stringResponse);
            return response.Choices[0].Message.Content;
        }

        public static async Task<string> OpenAIComplete(string apiKey, string endPoint, 
            string modelType, Message[] prompt, int maxTokens, double temprature)
        {
            var requestBody = new
            {
                model = modelType,
                messages = prompt,
                max_tokens = maxTokens,
                temperature = temprature
            };
            string jsonPayload = JsonConvert.SerializeObject(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, endPoint);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var httpResponse = await client.SendAsync(request);
            string responseContent = await httpResponse.Content.ReadAsStringAsync();

            return responseContent;
        }
    }
}
