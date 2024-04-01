using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GPT_API_Test
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        public struct Config
        {
            public string apiKey;
            public string endpointURL;
            public string modelType;
            public int maxTokens;
            public double temprature;
        }

        public struct Message
        {
            public string role;
            public string content;
        }

        public struct Choices
        {
            public Message message;
        }

        public struct Response
        {
            public Choices[] choices;
        }

        public static async Task Main(string[] args)
        {
            string configString = File.ReadAllText("openai.config");
            Config config = JsonConvert.DeserializeObject<Config>(configString);
            await StartChat(config);
        }

        public static async Task StartChat(Config config)
        {
            Console.WriteLine("Type something and press enter:\n");
            string input = Console.ReadLine();
            Console.WriteLine("\n[Receiving Response...]\n");
            string response = await OpenAISendPrompt(input, config);
            Console.WriteLine(response + "\n");
            await StartChat(config);
        }

        public static async Task<string> OpenAISendPrompt(string prompt, Config config)
        {
            Message[] prompts = new Message[1] { new Message { role = "user", content = prompt } };
            string stringResponse =
                await OpenAIComplete(prompts, config);
            Response response = JsonConvert.DeserializeObject<Response>(stringResponse);
            return response.choices[0].message.content;
        }

        public static async Task<string> OpenAIComplete(Message[] prompt, Config config)
        {
            var requestBody = new
            {
                model = config.modelType,
                messages = prompt,
                max_tokens = config.maxTokens,
                temperature = config.temprature
            };
            string jsonPayload = JsonConvert.SerializeObject(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, config.endpointURL);
            request.Headers.Add("Authorization", $"Bearer {config.apiKey}");
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var httpResponse = await client.SendAsync(request);
            string responseContent = await httpResponse.Content.ReadAsStringAsync();

            return responseContent;
        }
    }
}