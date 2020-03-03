using System.Threading.Tasks;
using Laso.AdminPortal.Web.Api.Notifications;
using Laso.AdminPortal.Web.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Laso.AdminPortal.UnitTests.Api.Notifications
{
    public class NotificationsControllerTests
    {
        [Fact]
        public async Task When_Post_Invoked_Should_Echo_Message()
        {
            // Arrange
            var hubContext = Substitute.For<IHubContext<NotificationsHub>>();
            var controller = new NotificationsController(hubContext);

            const string Message = "Something happened!";

            // Act
            var result = await controller.Post(Message);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBeOfType<OkResult>();
        }
    }
}
