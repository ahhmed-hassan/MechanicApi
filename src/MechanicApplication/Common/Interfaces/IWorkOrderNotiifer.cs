namespace MechanicApplication;

public interface IWorkOrderNotiifer
{
    Task NotifiyWorkOrdersChangedAsync(CancellationToken ct = default);

}
