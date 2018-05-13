// -----------------------------------------------------------------------
// <copyright file="StatusResponse.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace BotRepository.Client
{
    using Newtonsoft.Json;

    /// <summary>
    /// Provides information about status.
    /// </summary>
    public class StatusResponse
    {
        /// <summary>
        /// Gets or sets status of the operation.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets text message.
        /// </summary>
        [JsonProperty("txt")]
        public string Message { get; set; }
    }
}
