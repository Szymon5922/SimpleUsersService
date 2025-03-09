using Data.Repositories;
using System.Text.RegularExpressions;

namespace Application.Helpers
{
    public static class Validation
    {
        public static bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
                return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
        public static async Task<bool> IsEmailInUseAsync(IUserRepository userRepository, string email)
        {
            var userWithEmail = await userRepository.GetByEmailAsync(email);

            return userWithEmail is not null;
        }
        public static bool IsValidPostalCode(string postalCode)
        {
            if (string.IsNullOrWhiteSpace(postalCode))
                return false;

            string pattern = "^\\d{2}-\\d{3}$";
            return Regex.IsMatch(postalCode, pattern);
        }
    }
}
