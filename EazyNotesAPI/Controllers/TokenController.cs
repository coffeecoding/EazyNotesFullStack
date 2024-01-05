using Microsoft.AspNetCore.Mvc;
using static EazyNotesAPI.Internal.TokenUtils;
using EazyNotes.Models.DTO;
using System;
using EazyNotesAPI.Library.DataAccess;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace EazyNotesAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        [Route("token")]
        [HttpPost]
        [Consumes("application/json")]
        public IActionResult Create(AuthenticatingUser user)
        {
            try
            {
                if (IsValidCredentials(user.EmailOrUsername, user.PasswordHash))
                {
                    // if user is logging in with email, get the username because we want that one on the token and
                    // because we will use that one to create an entry in clients table
                    UserDataAccess userDataAccess = new UserDataAccess();
                    UserDTO userDTO = userDataAccess.GetUserByEmailOrUsername(user.EmailOrUsername);
                    ObjectResult objectResult = new ObjectResult(GenerateToken(userDTO.Username, userDTO.Id));

                    return objectResult;
                }
                else
                {
                    return BadRequest("Invalid user credentials.");
                }
            } catch (Exception e)
            {
                Console.WriteLine($"Exception in TokenController.Create: {e.GetType()} {e.Message}");
                return new BadRequestResult();
            }
        }

    }
}
