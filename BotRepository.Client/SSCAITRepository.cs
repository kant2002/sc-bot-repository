// -----------------------------------------------------------------------
// <copyright file="SSCAITRepository.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace BotRepository.Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Class which provides access to SSCAIT bot repository.
    /// </summary>
    public class SSCAITRepository
    {
        private readonly Uri uri;
        private readonly HttpClient client;
        private readonly CookieContainer cookieContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SSCAITRepository"/> class with given URI.
        /// </summary>
        /// <param name="uri">Uri address of the server where bot repository is hosted.</param>
        public SSCAITRepository(Uri uri)
        {
            this.uri = uri;
            this.cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = this.cookieContainer };
            this.client = new HttpClient(handler) { BaseAddress = this.uri };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SSCAITRepository"/> class with given URI.
        /// </summary>
        /// <param name="uri">Uri address of the server where bot repository is hosted.</param>
        public SSCAITRepository(string uri)
            : this(new Uri(uri))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SSCAITRepository"/> class with SSCAIT address.
        /// </summary>
        public SSCAITRepository()
            : this(new Uri("https://sscaitournament.com"))
        {
        }

        /// <summary>
        /// Performs login to the bot repository server.
        /// </summary>
        /// <param name="login">Login of the user. Use your email for SSCAIT server.</param>
        /// <param name="unsecurePassword">Password of the user.</param>
        /// <returns>A <see cref="Task"/> which provide inforamation about operation status.</returns>
        public async Task<StatusResponse> LoginAsync(string login, string unsecurePassword)
        {
            var password = new SecureString();
            foreach (var c in unsecurePassword)
            {
                password.AppendChar(c);
            }

            return await this.LoginAsync(login, password).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs login to the bot repository server.
        /// </summary>
        /// <param name="login">Login of the user. Use your email for SSCAIT server.</param>
        /// <param name="password">Password of the user.</param>
        /// <returns>A <see cref="Task"/> which provide inforamation about operation status.</returns>
        public async Task<StatusResponse> LoginAsync(string login, SecureString password)
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
            var response = await responseTask.ConfigureAwait(false);
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

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseObject = JsonConvert.DeserializeObject<StatusResponse>(responseString);
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

        /// <summary>
        /// Asynchronously upload bot.
        /// </summary>
        /// <param name="botName">Name of the bot on the SSCAIT server.</param>
        /// <param name="botFile">Name of the file with bot content.</param>
        /// <returns>A <see cref="Task"/> which provide inforamation about operation status.</returns>
        public async Task<StatusResponse> UploadAsync(string botName, string botFile)
        {
            return await this.UploadAsync(botName, new ByteArrayContent(File.ReadAllBytes(botFile))).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously upload bot.
        /// </summary>
        /// <param name="botName">Name of the bot on the SSCAIT server.</param>
        /// <param name="botBytes">Bytes with bot archive.</param>
        /// <returns>A <see cref="Task"/> which provide inforamation about operation status.</returns>
        public async Task<StatusResponse> UploadAsync(string botName, byte[] botBytes)
        {
            return await this.UploadAsync(botName, new ByteArrayContent(botBytes)).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously upload bot.
        /// </summary>
        /// <param name="botName">Name of the bot on the SSCAIT server.</param>
        /// <param name="botStream">Stream with bot archive.</param>
        /// <returns>A <see cref="Task"/> which provide inforamation about operation status.</returns>
        public async Task<StatusResponse> UploadAsync(string botName, Stream botStream)
        {
            return await this.UploadAsync(botName, new StreamContent(botStream)).ConfigureAwait(false);
        }

        private async Task<StatusResponse> UploadAsync(string botName, HttpContent botFileContent)
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
            botFileContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-zip-compressed");
            content.Add(botFileContent, "bot_binary", "bot.zip");

            var response = await this.client.PostAsync("https://sscaitournament.com/users/users/new_binary_submit.php", content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseObject = JsonConvert.DeserializeObject<StatusResponse>(responseString);
            if (responseObject.Status == 0)
            {
                return responseObject;
            }

            return responseObject;
        }
    }
}
