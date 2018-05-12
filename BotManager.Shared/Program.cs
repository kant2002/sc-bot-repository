using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace BotManager
{
    [Command(ThrowOnUnexpectedArgument = false)]
    [Subcommand("login", typeof(LoginCommand))]
    [Subcommand("upload", typeof(UploadCommand))]
    class Program
    {
        public static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<Program>(args);

        [Argument(0)]

        public string CommandName { get; set; }
        public string[] RemainingArgs { get; set; }

        private async Task<int> OnExecute()
        {
            const string prompt = @"
login  - Login to the SSCAIT server
upload - Upload the bot binary to server
> ";
            switch (this.CommandName)
            {
                case "login":
                    return await CommandLineApplication.ExecuteAsync<LoginCommand>(this.RemainingArgs);
                case "upload":
                    return await CommandLineApplication.ExecuteAsync<UploadCommand>(this.RemainingArgs);
                default:
                    Console.Error.WriteLine(prompt);
                    return 1;

            }

        }
    }
}
