

using ErrorOr;
using MechanicDomain.WorkOrders.Enums;
using MediatR;

namespace MechanicApplication.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed record CreateWorkOrderCommand(Spot Spot,
                                            Guid VehicleId,
                                            DateTimeOffset StartAt,
                                            List<Guid> RepairTasksIds,
                                            Guid LaborId)
    :IRequest<ErrorOr<Dtos.WorkOrderDTO>>;
   
   