using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application;
using Application.Exceptions;
using Application.Helpers;
using Application.Models;
using Application.Services;
using AutoMapper;
using Data.Entities;
using Data.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace SimpleUsersService
{
    public class UsersServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IAddressRepository> _addressRepositoryMock = new();
        private readonly Mock<IRoleRepository> _roleRepositoryMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly UsersService _usersService;

        public UsersServiceTests()
        {
            _usersService = new UsersService(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _addressRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task GetByIdAsync_UserExists_ReturnsUserDto()
        {
            var userId = 1;
            var user = new User { Id = userId, FirstName = "John" };
            var userDto = new UserDto { Id = userId, FirstName = "John" };

            _userRepositoryMock.Setup(repo => repo.GetByIDAsync(userId)).ReturnsAsync(user);
            _mapperMock.Setup(mapper => mapper.Map<UserDto>(user)).Returns(userDto);

            var result = await _usersService.GetByIdAsync(userId);

            result.Should().BeEquivalentTo(userDto);
        }

        [Fact]
        public async Task GetByIdAsync_UserDoesNotExist_ThrowsNotFoundException()
        {
            _userRepositoryMock.Setup(repo => repo.GetByIDAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            Func<Task> act = async () => await _usersService.GetByIdAsync(1);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage(Messages.UserNotFound);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsListOfUsers()
        {
            var users = new List<User> { new User { Id = 1 }, new User { Id = 2 } };
            var userDtos = users.Select(u => new UserDto { Id = u.Id }).ToList();

            _userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(users);
            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<UserDto>>(users)).Returns(userDtos);

            var result = await _usersService.GetAllAsync();

            result.Should().BeEquivalentTo(userDtos);
        }

        [Fact]
        public async Task GetPaginatedAsync_ValidPageAndLimit_ReturnsPaginatedUsers()
        {
            var page = 1;
            var limit = 10;
            var users = new List<User>
            {
                new User { Id = 1, FirstName = "John" },
                new User { Id = 2, FirstName = "Jane" }
            };
            var userDtos = users.Select(u => new UserDto { Id = u.Id, FirstName = u.FirstName }).ToList();

            _userRepositoryMock.Setup(repo => repo.GetPaginatedAsync(page, limit))
                .ReturnsAsync((users, users.Count));

            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<UserDto>>(users)).Returns(userDtos);

            var (resultUsers, totalCount) = await _usersService.GetPaginatedAsync(page, limit);

            resultUsers.Should().BeEquivalentTo(userDtos);
            totalCount.Should().Be(users.Count);
        }

        [Fact]
        public async Task AddAsync_ValidUser_ReturnsUserId()
        {
            var userDto = new ManipulateUserDto { Email = "valid@test.com" };
            var user = new User { Id = 1 };

            _mapperMock.Setup(m => m.Map<User>(userDto)).Returns(user);
            _roleRepositoryMock.Setup(r => r.GetDefaultRoleAsync()).ReturnsAsync(new Role());
            _userRepositoryMock.Setup(repo => repo.AddAsync(user)).Returns(Task.CompletedTask);

            var result = await _usersService.AddAsync(userDto);

            result.Should().Be(user.Id);
        }

        [Fact]
        public async Task AddAsync_InvalidEmail_ThrowsBadRequestException()
        {
            var userDto = new ManipulateUserDto { Email = "invalidemail" };

            Func<Task> act = async () => await _usersService.AddAsync(userDto);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage(Messages.InvalidEmail);
        }

        [Fact]
        public async Task UpdateAsync_UserExists_UpdatesUser()
        {
            var userId = 1;
            var userDto = new ManipulateUserDto { Email = "updated@test.com" };
            var user = new User { Id = userId, Email = "old@test.com" };

            _userRepositoryMock.Setup(repo => repo.GetByIDAsync(userId)).ReturnsAsync(user);
            _userRepositoryMock.Setup(repo => repo.UpdateAsync(user)).Returns(Task.CompletedTask);

            await _usersService.UpdateAsync(userId, userDto);

            user.Email.Should().Be(userDto.Email);
        }

        [Fact]
        public async Task UpdateAsync_UserDoesNotExist_ThrowsNotFoundException()
        {
            _userRepositoryMock.Setup(repo => repo.GetByIDAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            Func<Task> act = async () => await _usersService.UpdateAsync(1, new ManipulateUserDto());

            await act.Should().ThrowAsync<NotFoundException>().WithMessage(Messages.UserNotFound);
        }

        [Fact]
        public async Task DeleteAsync_UserExists_DeletesUser()
        {
            var user = new User { Id = 1 };
            _userRepositoryMock.Setup(repo => repo.GetByIDAsync(user.Id)).ReturnsAsync(user);

            await _usersService.DeleteAsync(user.Id);

            _userRepositoryMock.Verify(repo => repo.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_UserDoesNotExist_ThrowsNotFoundException()
        {
            _userRepositoryMock.Setup(repo => repo.GetByIDAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            Func<Task> act = async () => await _usersService.DeleteAsync(1);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage(Messages.UserNotFound);
        }

        [Fact]
        public async Task DeleteAddressAsync_UserDoesNotExist_ThrowsNotFoundException()
        {
            _userRepositoryMock.Setup(repo => repo.GetByIDAsync(It.IsAny<int>())).ReturnsAsync((User)null);

            Func<Task> act = async () => await _usersService.DeleteAddressAsync(1, 1);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage(Messages.UserNotFound);
        }

        [Fact]
        public async Task DeleteAddressAsync_AddressNotInUser_ThrowsNotFoundException()
        {
            var user = new User { Id = 1, Addresses = new List<Address>() };
            _userRepositoryMock.Setup(repo => repo.GetByIDAsync(user.Id)).ReturnsAsync(user);

            Func<Task> act = async () => await _usersService.DeleteAddressAsync(1, 1);

            await act.Should().ThrowAsync<NotFoundException>().WithMessage(Messages.AddresNotInUser);
        }
        [Fact]
        public async Task DeleteAddressAsync_AddressExists_DeletesAddress()
        {
            var user = new User { Id = 1, Addresses = new List<Address> { new Address { Id = 1 } } };
            var address = user.Addresses.First();

            _userRepositoryMock.Setup(repo => repo.GetByIDAsync(user.Id)).ReturnsAsync(user);
            _addressRepositoryMock.Setup(repo => repo.DeleteAsync(address)).Returns(Task.CompletedTask);

            await _usersService.DeleteAddressAsync(user.Id, address.Id);

            _addressRepositoryMock.Verify(repo => repo.DeleteAsync(address), Times.Once);
        }
    }
}