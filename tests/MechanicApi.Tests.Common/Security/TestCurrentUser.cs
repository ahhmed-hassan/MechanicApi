using MechanicApplication.Common.Interfaces;
using MechanicInfrastructure.Identity;

namespace MechanicShop.Tests.Common.Security;

public class TestCurrentUser : IUser
{
    private AppUser? _currentUser;

    public void Returns(AppUser currentUser)
    {
        _currentUser = currentUser;
    }

    public string? Id => _currentUser!.Id ?? UserFactory.DummyUser.Id;
}