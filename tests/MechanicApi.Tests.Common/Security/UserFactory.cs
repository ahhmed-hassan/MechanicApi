using MechanicInfrastructure.Identity;

namespace MechanicShop.Tests.Common.Security;

internal class UserFactory
{
    public static AppUser DummyUser => 
        new AppUser
        {
            Id = "00000000-0000-0000-0000-000000000000",
            Email = "dummy@localhost",
            UserName = "dummy@localhost",
            EmailConfirmed = true
        };

}