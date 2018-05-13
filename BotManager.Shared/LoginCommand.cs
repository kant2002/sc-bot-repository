using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BotRepository.Client;
using System.Security;

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

            SecureString password = null;
            while (password == null || password.Length == 0)
            {
                Console.Write("Enter password: ");
                if (password != null)
                {
                    password.Dispose();
                    password = null;
                }

                password = GetHiddenConsoleInput();
                Console.WriteLine();
            }

            var repository = new SSCAITRepository();
            try
            {
                var responseObject = await repository.LoginAsync(login, password);
                if (responseObject.Status == 0)
                {
                    Console.WriteLine($"Server return error: {responseObject.Message}");
                    return 1;
                }

                var bwbotDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    ".bwbot");
                var cookiesFile = Path.Combine(
                    bwbotDirectory,
                    ".auth");
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

        private static SecureString GetHiddenConsoleInput()
        {
            var ss = new SecureString();
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key == ConsoleKey.Backspace && ss.Length > 0) ss.RemoveAt(ss.Length - 1);
                else if (key.Key != ConsoleKey.Backspace) ss.AppendChar(key.KeyChar);
            }

            ss.MakeReadOnly();
            return ss;
        }
    }
}
