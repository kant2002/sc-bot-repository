// -----------------------------------------------------------------------
// <copyright file="SecurityUtils.cs" company="Andrii Kurdiumov">
// Copyright (c) Andrii Kurdiumov. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace BotRepository.Client
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Security utilities.
    /// </summary>
    internal static class SecurityUtils
    {
         /// <summary>
         /// Passes decrypted password String pinned in memory to Func delegate scrubbed on return.
         /// </summary>
         /// <typeparam name="T">Generic type returned by Func delegate.</typeparam>
         /// <param name="secureString">Secure string on which operation should be performed.</param>
         /// <param name="action">Func delegate which will receive the decrypted password pinned in memory as a String object.</param>
         /// <returns>Result of Func delegate.</returns>
        public static T DecryptSecureString<T>(SecureString secureString, Func<string, T> action)
        {
            var insecureStringPointer = IntPtr.Zero;
            var insecureString = string.Empty;
            var gcHandler = GCHandle.Alloc(insecureString, GCHandleType.Pinned);

            try
            {
                insecureStringPointer = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                insecureString = Marshal.PtrToStringUni(insecureStringPointer);

                return action(insecureString);
            }
            finally
            {
                insecureString = null;

                gcHandler.Free();
                Marshal.ZeroFreeGlobalAllocUnicode(insecureStringPointer);
            }
        }

        /// <summary>
        /// Runs DecryptSecureString with support for Action to leverage void return type.
        /// </summary>
        /// <param name="secureString">Secure string on which operation should be performed.</param>
        /// <param name="action">Function which will receive the decrypted password pinned in memory as a String object.</param>
        public static void DecryptSecureString(SecureString secureString, Action<string> action)
        {
            DecryptSecureString<int>(secureString, (s) =>
            {
                action(s);
                return 0;
            });
        }
    }
}
