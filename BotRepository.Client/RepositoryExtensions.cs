namespace BotRepository.Client
{
    using System.IO;
    using System.Security;
    using System.Threading.Tasks;

    public static class RepositoryExtensions
    {
        public static LoginResponse Login(this SSCAITRepository repository, string login, SecureString password)
        {
            return Task.Run(() => repository.LoginAsync(login, password)).Result;
        }

        public static LoginResponse Login(this SSCAITRepository repository, string login, string unsecurePassword)
        {
            return Task.Run(() => repository.LoginAsync(login, unsecurePassword)).Result;
        }

        public static LoginResponse Upload(this SSCAITRepository repository, string botName, string botFile)
        {
            return Task.Run(() => repository.UploadAsync(botName, botFile)).Result;
        }

        public static LoginResponse Upload(this SSCAITRepository repository, string botName, byte[] botBytes)
        {
            return Task.Run(() => repository.UploadAsync(botName, botBytes)).Result;
        }

        public static LoginResponse Upload(this SSCAITRepository repository, string botName, Stream botStream)
        {
            return Task.Run(() => repository.UploadAsync(botName, botStream)).Result;
        }
    }
}
