using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Chapter3.Examples.LOH.Web.Controllers
{
    /** Different ways of getting an image data from the POST.
     */
    [ApiController]
    [Route("[controller]")]
    public class ImagingController : ControllerBase
    {
        private readonly ILogger<ImagingController> _logger;

        // The Web API will only accept tokens 1) for users, and 2) having the "access_as_user" scope for this API
        static readonly string[] scopeRequiredByApi = new string[] { "access_as_user" };

        public ImagingController(ILogger<ImagingController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var body = reader.ReadToEnd();
                // Do something
            }
            return "Ok";
        }

        [HttpGet]
        public async Task<string> GetAsync()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                // Do something
            }
            return "Ok";
        }

        [HttpGet]
        public async Task<byte[]> GetBytesAsync()
        {
            using (var ms = new MemoryStream(2048))
            {
                await Request.Body.CopyToAsync(ms);
                return ms.ToArray();  // returns base64 encoded string JSON result
            }
        }
    }
}
