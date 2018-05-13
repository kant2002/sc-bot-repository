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
using BotRepository.Client;

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
                return 2;
            }

            var repository = new SSCAITRepository();
            try
            {
                var responseObject = await repository.UploadAsync(this.BotName, this.FileName);
                if (responseObject.Status == 0)
                {
                    Console.WriteLine($"Server return error: {responseObject.Txt}");
                    return 1;
                }

                Console.WriteLine($"Bot uploaded");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to server");
                Console.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}
