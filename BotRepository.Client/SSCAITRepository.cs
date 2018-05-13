using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading.Tasks;

namespace BotRepository.Client
{
    public class SSCAITRepository
    {
        private readonly Uri uri;
        private readonly HttpClient client;
        private readonly CookieContainer cookieContainer;

        public SSCAITRepository(Uri uri)
        {
            this.uri = uri;
            this.cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            this.client = new HttpClient(handler) { BaseAddress = this.uri };
        }

        public SSCAITRepository(string uri)
            : this(new Uri(uri))
        {
        }

        public SSCAITRepository()
            : this(new Uri("https://sscaitournament.com"))
        {
        }

        public async Task<LoginResponse> LoginAsync(string login, string unsecurePassword)
        {
            var password = new SecureString();
            foreach (var c in unsecurePassword)
            {
                password.AppendChar(c);
            }

            return await this.LoginAsync(login, password);
        }

        public async Task<LoginResponse> LoginAsync(string login, SecureString password)
        {
            var responseTask = SecurityUtils.DecryptSecureString(password, (decryptedPassword) =>
            {
                var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("username", login),
                    new KeyValuePair<string, string>("password", decryptedPassword),
                });
                return this.client.PostAsync("https://sscaitournament.com/users/login_submit.php", content);
            });
            var response = await responseTask;
            response.EnsureSuccessStatusCode();

            if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                throw new InvalidOperationException($"Authentication error. Make sure that you enter correct password");
            }

            var cookie = cookies.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(cookie))
            {
                throw new InvalidOperationException($"Authentication error. Make sure that you enter correct password");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<LoginResponse>(responseString);
            if (responseObject.Status == 0)
            {
                return responseObject;
            }

            var bwbotDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ".bwbot");
            Directory.CreateDirectory(bwbotDirectory);
            var cookiesFile = Path.Combine(
                bwbotDirectory,
                ".auth");
            File.WriteAllLines(cookiesFile, cookies.Select(_ => _.Split(';').First()));
            return responseObject;
        }

        public async Task<LoginResponse> UploadAsync(string botName, string botFile)
        {
            var bwbotDirectory = Path.Combine(
                   Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                   ".bwbot");
            var cookiesFile = Path.Combine(
                bwbotDirectory,
                ".auth");
            if (File.Exists(cookiesFile) && this.cookieContainer.GetCookies(this.uri).Count == 0)
            {
                var cookes = File.ReadAllLines(cookiesFile);
                foreach (var c in cookes)
                {
                    var parts = c.Split(new char[] { '=' }, 2);
                    this.cookieContainer.Add(this.uri, new Cookie(parts[0], parts[1]));
                }
            }

            var content = new MultipartFormDataContent();
            content.Add(new StringContent(botName), "user");
            var botFileContent = new ByteArrayContent(File.ReadAllBytes(botFile));
            botFileContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-zip-compressed");
            content.Add(botFileContent, "bot_binary", "bot.zip");

            var response = await this.client.PostAsync("https://sscaitournament.com/users/users/new_binary_submit.php", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<LoginResponse>(responseString);
            if (responseObject.Status == 0)
            {
                return responseObject;
            }

            return responseObject;
        }
    }
}
