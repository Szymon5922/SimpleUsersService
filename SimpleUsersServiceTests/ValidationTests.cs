using Application.Helpers;
using Application.Services;
using AutoMapper;
using Data.Entities;
using Data.Repositories;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleUsersServiceTests
{
    public class ValidationTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();

        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("invalidemail", false)]
        [InlineData(" test@example.com ", true)]
        public void IsValidEmail_ValidatesCorrectly(string email, bool expected)
        {
            var result = Validation.IsValidEmail(email);

            result.Should().Be(expected);
        }

        [Fact]
        public async Task IsEmailInUseAsync_EmailExists_ReturnsTrue()
        {
            var email = "test@example.com";
            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(email)).ReturnsAsync(new User { Email = email });

            var result = await Validation.IsEmailInUseAsync(_userRepositoryMock.Object, email);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsEmailInUseAsync_EmailDoesNotExist_ReturnsFalse()
        {
            var email = "test@example.com";
            _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(email)).ReturnsAsync((User)null);

            var result = await Validation.IsEmailInUseAsync(_userRepositoryMock.Object, email);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("12-345", true)]
        [InlineData("12345", false)]
        [InlineData("1-2345", false)]
        [InlineData("12-34a", false)]
        [InlineData("", false)]
        public void IsValidPostalCode_ValidatesCorrectly(string postalCode, bool expected)
        {
            var result = Validation.IsValidPostalCode(postalCode);

            result.Should().Be(expected);
        }
    }
}
