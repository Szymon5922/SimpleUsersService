namespace Application.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public List<AddressDto> Addresses { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
