// -----------------------------------------------------------------------
// <copyright file="RepositoryExtensions.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace BotRepository.Client
{
    using System.IO;
    using System.Security;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for SSCAIT repository to provide synchronous API.
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Performs login to the bot repository server.
        /// </summary>
        /// <param name="repository">SSCAIT repository on which perform operation.</param>
        /// <param name="login">Login of the user. Use your email for SSCAIT server.</param>
        /// <param name="password">Password of the user.</param>
        /// <returns>Information about operation status.</returns>
        public static StatusResponse Login(this SSCAITRepository repository, string login, SecureString password)
        {
            return Task.Run(() => repository.LoginAsync(login, password)).Result;
        }

        /// <summary>
        /// Performs login to the bot repository server.
        /// </summary>
        /// <param name="repository">SSCAIT repository on which perform operation.</param>
        /// <param name="login">Login of the user. Use your email for SSCAIT server.</param>
        /// <param name="unsecurePassword">Password of the user.</param>
        /// <returns>Information about operation status.</returns>
        public static StatusResponse Login(this SSCAITRepository repository, string login, string unsecurePassword)
        {
            return Task.Run(() => repository.LoginAsync(login, unsecurePassword)).Result;
        }

        /// <summary>
        /// Upload archive with bot content.
        /// </summary>
        /// <param name="repository">SSCAIT repository on which perform operation.</param>
        /// <param name="botName">Name of the bot on the SSCAIT server.</param>
        /// <param name="botFile">Name of the file with bot content.</param>
        /// <returns>Information about operation status.</returns>
        public static StatusResponse Upload(this SSCAITRepository repository, string botName, string botFile)
        {
            return Task.Run(() => repository.UploadAsync(botName, botFile)).Result;
        }

        /// <summary>
        /// Upload archive with bot content.
        /// </summary>
        /// <param name="repository">SSCAIT repository on which perform operation.</param>
        /// <param name="botName">Name of the bot on the SSCAIT server.</param>
        /// <param name="botBytes">Bytes with bot archive.</param>
        /// <returns>Information about operation status.</returns>
        public static StatusResponse Upload(this SSCAITRepository repository, string botName, byte[] botBytes)
        {
            return Task.Run(() => repository.UploadAsync(botName, botBytes)).Result;
        }

        /// <summary>
        /// Upload archive with bot content.
        /// </summary>
        /// <param name="repository">SSCAIT repository on which perform operation.</param>
        /// <param name="botName">Name of the bot on the SSCAIT server.</param>
        /// <param name="botStream">Stream with bot archive.</param>
        /// <returns>Information about operation status.</returns>
        public static StatusResponse Upload(this SSCAITRepository repository, string botName, Stream botStream)
        {
            return Task.Run(() => repository.UploadAsync(botName, botStream)).Result;
        }
    }
}
