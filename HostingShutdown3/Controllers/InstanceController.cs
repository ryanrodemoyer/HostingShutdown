using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HostingShutdown3.Controllers
{
    [RoutePrefix("api/instance")]
    public class InstanceController : ApiController
    {
        [Route("")]
        public string Get()
        {
            var instance = SharedResource.Instance;
            return "hey there ; ";
        }
    }
}
