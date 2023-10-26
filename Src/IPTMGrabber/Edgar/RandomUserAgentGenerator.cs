using System.Text;

namespace IPTMGrabber.Edgar
{
    internal class RandomUserAgentGenerator
    {
        private static Random random = new Random();

        public static string Next()
        {
            string[] clients = { "MonClient", "MyApp", "CoolApp", "AwesomeClient" };
            string[] domains = { "gmail.com", "yahoo.com", "hotmail.com", "outlook.com" };

            // Générez un préfixe aléatoire
            string prefix = clients[random.Next(clients.Length)];

            // Générez une adresse e-mail aléatoire
            string username = GenerateRandomString(8); // 8 caractères aléatoires
            string domain = domains[random.Next(domains.Length)];

            string email = $"{username}@{domain}";

            // Générez un User-Agent avec le préfixe et l'adresse e-mail
            string userAgent = $"{prefix}/1.0 ({email})";

            return userAgent;
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                char randomChar = chars[random.Next(chars.Length)];
                builder.Append(randomChar);
            }

            return builder.ToString();
        }
    }
}
