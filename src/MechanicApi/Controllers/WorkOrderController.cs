using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace MechanicApi.Controllers
{
    [Authorize]
    [Route("api/v{version:apiVersion}/workorders")]
    [ApiVersion("1.0")]
    public class WorkOrderController : ApiBaseController
    {
        
    }
}
