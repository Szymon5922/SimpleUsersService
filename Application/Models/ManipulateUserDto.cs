namespace Application.Models
{
    public class ManipulateUserDto //dto using while creating/updating user
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime DateOfBirth { get; set; }        
    }
}
