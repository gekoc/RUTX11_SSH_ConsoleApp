using AutoFixture.Xunit2;
using Moq;
using RUTX11_SSH_ConsoleApp;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace RUTX11_SSH_ConsoleApp_Tests
{
    public class App_Should
    {
        private readonly App _sut;
        private readonly Mock<IUserLog> _userLogMock = new Mock<IUserLog>();
        private readonly Mock<IConsoleIO> _consoleMock = new Mock<IConsoleIO>();
        private readonly Mock<IRouter> _routerMock = new Mock<IRouter>();

        public App_Should()
        {
            _sut = new App(_userLogMock.Object, _routerMock.Object);
        }

        [Theory]
        [AutoData]
        public void DoesNot_Save_Authentication_Error_When_AuthenticateUser_Can_Connect_to_Router(User user)
        {
            var attempt = new Attempt();
            var userId = user.Id;
            _routerMock.Setup(x => x.CanConnectToRouter(user)).Returns(true);
            

            _sut.AuthenticateUser(user);
            _userLogMock.Verify(x => x.LogAuthenticationError(attempt), Times.Never);
        }

        [Theory]
        [AutoData]
        public void Return_User_When_AuthenticateUser(User user)
        {
            var attempt = new Attempt();
            var userId = user.Id;
            _routerMock.Setup(x => x.CanConnectToRouter(user)).Returns(true);

            var user1 = _sut.AuthenticateUser(user);

            var expectedUser = user;

            Assert.Equal(expectedUser.Id, user1.Id);

        }

        [Theory]
        [AutoData]
        public void Print_Error_Messages_When_GetSessionReport(User user, Attempt attempt, Error error)
        {
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            _userLogMock.Setup(x => x.GetUserAttempts(user)).Returns(new List<Attempt> { attempt });
            _userLogMock.Setup(x => x.GetAttemptErrors(attempt)).Returns(new List<Error> { error });

            _sut.GetSessionReport(user, attempt);

            var actualOutput = stringWriter.ToString();
            Assert.Contains(error.Message, actualOutput);
        }

        [Theory]
        [AutoData]
        public void Not_Print_Erros_When_GetSessionReport_And_No_Errors(User user, Attempt attempt)
        {
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            _userLogMock.Setup(x => x.GetUserAttempts(user)).Returns(new List<Attempt> { attempt });
            _userLogMock.Setup(x => x.GetAttemptErrors(attempt)).Returns(new List<Error>());

            _sut.GetSessionReport(user, attempt);

            var actualOutput = stringWriter.ToString();
            Assert.DoesNotContain("The errors ecountered are:", actualOutput);
        }
    }
}
