using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BotManager
{
    [HelpOption("--help")]
    class UploadCommand
    {
        [Required]
        [Option("-f|--bot-file", Description = "[Required] Path to ZIP archive with bot files")]
        [FileExists]
        public string FileName { get; set; }

        [Required]
        [Option("-b|--bot-name", Description = "[Required] Name of the bot")]
        public string BotName { get; set; }

        protected async Task<int> OnExecute(CommandLineApplication app)
        {
            Console.WriteLine($"Uploading bot to server from file {this.FileName}");
            var bwbotDirectory = Path.Combine(
                   Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                   ".bwbot");
            var cookiesFile = Path.Combine(
                bwbotDirectory,
                ".auth");
            if (!File.Exists(cookiesFile))
            {
                Console.WriteLine("Please authenticate using bwbot login");
            }

            var cookes = File.ReadAllLines(cookiesFile);

            var baseAddress = new Uri("https://sscaitournament.com");
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            var client = new HttpClient(handler) { BaseAddress = baseAddress };
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(this.BotName), "user");
            var botFileContent = new ByteArrayContent(File.ReadAllBytes(this.FileName));
            botFileContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-zip-compressed");
            content.Add(botFileContent, "bot_binary", "bot.zip");
            foreach (var c in cookes)
            {
                var parts = c.Split(new char[] { '=' }, 2);
                cookieContainer.Add(baseAddress, new Cookie(parts[0], parts[1]));
            }

            var response = await client.PostAsync("https://sscaitournament.com/users/users/new_binary_submit.php", content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"The server generate error {response.StatusCode}");
                Console.WriteLine(response.ReasonPhrase);
                return 1;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<LoginResponse>(responseString);
            if (responseObject.Status == 0)
            {
                Console.WriteLine($"Server return error: {responseObject.Txt}");
                return 1;
            }

            Console.WriteLine($"Bot uploaded");
            return 0;
        }
    }
}
