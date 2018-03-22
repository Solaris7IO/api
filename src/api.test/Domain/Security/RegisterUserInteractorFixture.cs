﻿using Moq;
using OutreachOperations.Api.Domain;
using OutreachOperations.Api.Domain.Security;
using Xunit;

namespace OutreachOperations.Api.Test.Domain.Security
{
    public class RegisterUserInteractorFixture
    {
        [Fact]
        public void Execute_RegisterUser_RequiresPassword()
        {
            var interactor = RegisterUserInteractor(null, null, null,null);

            var result = interactor.Execute(
                GetRequest("email@email.com", "username", ""));

            Assert.Equal("Password is required to register",result.ResultMessage);
        }

        [Fact]
        public void Execute_RegisterUser_RequiresEmailAddress()
        {
            var interactor = RegisterUserInteractor(null, null, null,null);

            var result = interactor.Execute(GetRequest("", "username", "password"));

            Assert.Equal("An email address is required to register", result.ResultMessage);
        }

        [Fact]
        public void Execute_RegisterUser_RequiresUserName()
        {
            var interactor = RegisterUserInteractor(null, null, null,null);

            var result = interactor.Execute(GetRequest("email@email.com", "", "password"));

            Assert.Equal("An username is required to register", result.ResultMessage);
        }

        [Fact]
        public void Execute_UserNameFound_CannotCreateUser()
        {
            var userQuery = new Mock<FindUserQuery>();
            userQuery.Setup(x => x.Execute("username")).Returns(new User());

            var emailQuery = new Mock<FindUserQueryByEmail>();
            emailQuery.Setup(x => x.Execute("email@email.com")).Returns((User)null);

            var interactor = RegisterUserInteractor(userQuery.Object, emailQuery.Object, null, null);

            var result = interactor.Execute(GetRequest("email@email.com", "username", "password"));

            Assert.Equal("Username exists, please choose another", result.ResultMessage);

            userQuery.Verify();
        }

        [Fact]
        public void Execute_EmailAddressFound_CannotCreateUser()
        {
            var userQuery = new Mock<FindUserQuery>();
            userQuery.Setup(x => x.Execute("username")).Returns((User)null);

            var emailQuery = new Mock<FindUserQueryByEmail>();
            emailQuery.Setup(x => x.Execute("email@email.com")).Returns(new User());

            var interactor = RegisterUserInteractor(userQuery.Object, emailQuery.Object, null,null);

            var result = interactor.Execute(GetRequest("email@email.com", "username", "password"));

            Assert.Equal("Email address exists", result.ResultMessage);

            userQuery.Verify();
            emailQuery.Verify();
        }

        [Fact]
        public void Execute_UserNotFound_CreateUser()
        {
            var userQuery = new Mock<FindUserQuery>();
            userQuery.Setup(x => x.Execute("userName")).Returns((User) null);

            var emailQuery = new Mock<FindUserQueryByEmail>();
            emailQuery.Setup(x => x.Execute("email@email.com")).Returns((User) null);

            var repository = new Mock<IRepository>();
            repository.Setup(x => x.Insert(It.IsAny<User>()));

            var passwordHash = new Mock<PasswordHash>();
            passwordHash.Setup(x => x.HashPassword("password"));

            var interactor = RegisterUserInteractor(userQuery.Object, emailQuery.Object, repository.Object, passwordHash.Object);

            var result = interactor.Execute(GetRequest("email@email.com", "username", "password"));

            Assert.True(string.IsNullOrEmpty(result.ResultMessage));

            repository.Verify();
            userQuery.Verify();
            emailQuery.Verify();
        }

        private static RegistrationRequest GetRequest(string emailAddress, string username, string password)
        {
            return new RegistrationRequest
            {
                EmailAddress = emailAddress,
                Username = username,
                Password = password
            };
        }

        private static RegisterUserInteractor RegisterUserInteractor(FindUserQuery findUserQuery,
            FindUserQueryByEmail findUserQueryByEmail, IRepository repository,PasswordHash hash)
        {
            var interactor = new RegisterUserInteractor(findUserQuery, findUserQueryByEmail, repository, hash);
            return interactor;
        }

    }
}
