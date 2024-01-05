using EazyNotes.CryptoServices;
using EazyNotes.Models.DTO;
using EazyNotesAPI.Library.DataAccess;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EazyNotesAPI.Controllers
{
    [Route("api/salt")]
    [ApiController]
    public class UserSaltController : ControllerBase
    {
        [Route("{emailOrUsername?}")]
        [HttpGet]
        [Consumes("application/json")]
        public ActionResult<UserPWParams> GetSaltByEmailOrUsernam(string emailOrUsername)
        {
            // TODO: More specific error handling, validation and return messages!
            Console.WriteLine("Trying to get salt for user " + emailOrUsername);
            try
            {
                // TODO: take Guid userId as parameter, require JWT token to access this resource, then validate
                // token claim matching userId and only then return salt; that way salt is protected to user

                // Parse Username/Email (whatever user used to log in) from the JWT Token from the Request Header
                // Use that to grab full user info from database to return it
                UserDataAccess userDataAccess = new UserDataAccess();
                UserDTO user = userDataAccess.GetUserByEmailOrUsername(emailOrUsername);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                UserPWParams pwParams = new UserPWParams()
                {
                    PasswordSalt = user.PasswordSalt,
                    AlgorithmIdentifier = JsonSerializer.Deserialize<AlgorithmIdentifier>(user.AlgorithmIdentifier)
                };
                Console.WriteLine($"Retrieved salt successfully for {emailOrUsername}");
                return pwParams;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in GetSaltByEmailOrUsernam: {e.GetType()} : {e.Message}\nStackTrace: {e.StackTrace}");
                return BadRequest();
            }
        }
    }
}
