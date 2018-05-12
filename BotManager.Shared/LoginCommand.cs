using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BotManager
{
    [HelpOption("--help")]
    class LoginCommand
    {
        protected async Task<int> OnExecute(CommandLineApplication app)
        {
            Console.WriteLine("Please provide information to login on the SSCAIT server");
            string login = null;
            while (string.IsNullOrWhiteSpace(login))
            {
                Console.Write("Enter email: ");
                login = Console.ReadLine();
            }

            string password = null;
            while (string.IsNullOrWhiteSpace(password))
            {
                Console.Write("Enter password: ");
                password = GetHiddenConsoleInput();
                Console.WriteLine();
            }

            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("username", login),
                new KeyValuePair<string, string>("password", password),
            });
            try
            {
                var response = await client.PostAsync("https://sscaitournament.com/users/login_submit.php", content);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"The server generate error {response.StatusCode}");
                    Console.WriteLine(response.ReasonPhrase);
                    return 1;
                }

                if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    Console.WriteLine($"Authentication error. Make sure that you enter correct password");
                    return 1;
                }

                var cookie = cookies.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(cookie))
                {
                    Console.WriteLine($"Authentication error. Make sure that you enter correct password");
                    return 1;
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<LoginResponse>(responseString);
                if (responseObject.Status == 0)
                {
                    Console.WriteLine($"Server return error: {responseObject.Txt}");
                    return 1;
                }

                var bwbotDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    ".bwbot");
                Directory.CreateDirectory(bwbotDirectory);
                var cookiesFile = Path.Combine(
                    bwbotDirectory,
                    ".auth");
                File.WriteAllLines(cookiesFile, cookies.Select(_ => _.Split(';').First()));
                Console.WriteLine($"Authentication information saved to {cookiesFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to server");
                Console.WriteLine(ex.Message);
                return 1;
            }

            return 0;
        }
        private static string GetHiddenConsoleInput()
        {
            var input = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key == ConsoleKey.Backspace && input.Length > 0) input.Remove(input.Length - 1, 1);
                else if (key.Key != ConsoleKey.Backspace) input.Append(key.KeyChar);
            }
            return input.ToString();
        }
    }
}
