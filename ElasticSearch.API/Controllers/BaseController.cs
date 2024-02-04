using Elasticsearch.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Elasticsearch.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {

        [NonAction]
        public IActionResult CreateActionResult<T>(ResponseDto<T> response)
        {
            //200 ama response bodysi olmayabilir.
            if (response.Status == HttpStatusCode.NoContent)
                return new ObjectResult(null) { StatusCode = response.Status.GetHashCode() };//201 204 vs dönüyor.
            
            return new ObjectResult(response) { StatusCode = response.Status.GetHashCode() };

        }
    }
}
