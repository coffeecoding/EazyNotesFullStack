using EazyNotes.Models.DTO;
using EazyNotesAPI.Internal;
using EazyNotesAPI.Library.DataAccess;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EazyNotesAPI.Controllers
{
    [Route("api/client")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Post([FromBody] ClientDTO cl)
        {
            try
            {
                string username = HttpContext.User.Identity.Name;

                var clientAddress = HttpContext.Request.Host.Host;
                IPGeoData ipGeoData = await IPGeoClient.GetGeolocationByIP(clientAddress);

                // Add client if it is new
                ClientAccess ca = new ClientAccess();
                Client client = new Client(0, username, cl.DeviceName, cl.Platform, ipGeoData.Country_name, DateTime.UtcNow);
                ca.Insert(client);

                return Ok();
            } catch (Exception e)
            {
                Console.WriteLine($"Exception in ClientController.Post: {e.GetType()} {e.Message}");
                return BadRequest();
            }
        }
    }
}
