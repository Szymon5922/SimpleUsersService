using Application.Exceptions;
using Application.Helpers;
using Application.Models;
using AutoMapper;
using Data.Entities;
using Data.Repositories;

namespace Application.Services
{
    public interface IUsersService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<int> AddAsync(ManipulateUserDto user);
        Task UpdateAsync(int id, ManipulateUserDto user);
        Task DeleteAsync(int id);
        Task<int> AddAddressAsync(CreateAddressDto addressDto, int userId);
        Task DeleteAddressAsync(int userId, int addressId);
    }

    public class UsersService : IUsersService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public UsersService(IUserRepository userRepository, IRoleRepository roleRepository,
                            IAddressRepository addressRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _addressRepository = addressRepository;
            _mapper = mapper;
        }

        public async Task<UserDto> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIDAsync(id)
                       ?? throw new NotFoundException(Messages.UserNotFound);
            return _mapper.Map<UserDto>(user);
        }
        public async Task<User> GetByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<int> AddAsync(ManipulateUserDto userDto)
        {
            if (!Validation.IsValidEmail(userDto.Email))
                throw new BadRequestException(Messages.InvalidEmail);

            if (await Validation.IsEmailInUseAsync(_userRepository, userDto.Email))
                throw new BadRequestException(Messages.EmailInUse);

            var user = _mapper.Map<User>(userDto);
            user.CreatedAt = DateTime.UtcNow;
            user.Role = await _roleRepository.GetDefaultRoleAsync();

            await _userRepository.AddAsync(user);
            return user.Id;
        }

        public async Task UpdateAsync(int id, ManipulateUserDto userDto)
        {
            var user = await _userRepository.GetByIDAsync(id)
                       ?? throw new NotFoundException(Messages.UserNotFound);

            if (await Validation.IsEmailInUseAsync(_userRepository, userDto.Email))
                throw new BadRequestException(Messages.EmailInUse);

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Email = userDto.Email;
            user.PasswordHash = userDto.PasswordHash;
            user.DateOfBirth = userDto.DateOfBirth;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _userRepository.GetByIDAsync(id)
                       ?? throw new NotFoundException(Messages.UserNotFound);

            await _userRepository.DeleteAsync(user);
        }

        public async Task<int> AddAddressAsync(CreateAddressDto addressDto, int userId)
        {
            if (!Validation.IsValidPostalCode(addressDto.PostalCode))
                throw new BadRequestException(Messages.InvalidPostalCode);

            var user = await _userRepository.GetByIDAsync(userId)
                       ?? throw new NotFoundException(Messages.UserNotFound);

            var address = _mapper.Map<Address>(addressDto);
            address.User = user;

            await _addressRepository.AddAsync(address);
            return address.Id;
        }

        public async Task DeleteAddressAsync(int userId, int addressId)
        {
            var user = await _userRepository.GetByIDAsync(userId)
                       ?? throw new NotFoundException(Messages.UserNotFound);

            var address = user.Addresses.FirstOrDefault(a => a.Id == addressId)
                ?? throw new NotFoundException(Messages.AddresNotInUser);

            await _addressRepository.DeleteAsync(address);
        }

    }
}
