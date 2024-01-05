using EazyNotes.CryptoServices;
using EazyNotes.Models.DTO;
using EazyNotesAPI.Library.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace EazyNotesAPI.Controllers
{
    [Route("api/register")]
    [ApiController]
    public class UserRegistryController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post(UserDTO user)
        {
            UserDataAccess userDataAccess = new UserDataAccess();

            // Check if User already exists, return error if so
            UserDTO existingUser = userDataAccess.GetUserById(user.Id.ToString());
            if (existingUser != null)
            {
                // This is catching the UNLIKELY event that an existing Guid was seeded again.
                return BadRequest("Please try again.");
            }
            existingUser = userDataAccess.GetUserByEmailOrUsername(user.Username);
            if (existingUser != null)
            {
                ModelStateDictionary dict = new ModelStateDictionary();
                dict.AddModelError("Message", "Username already in use.");
                return BadRequest(dict);
            }
            existingUser = userDataAccess.GetUserByEmailOrUsername(user.Email);
            if (existingUser != null)
            {
                return BadRequest("User with this email address already exists.");
            }
            //if (!user.IsValid())
            //{
            //    return BadRequest("Invalid input.");
            //}

            user.PasswordHash = HashingHelper.PerformHash(user.PasswordHash, HashingHelper.CURRENT_VERSION);

            userDataAccess.InsertUser(user);
            // TODO: Verify No need to return these parameters ... ?
            //return Created(user.Id.ToString(), user);
            return Created(user.Username, user);
        }

        // TODO: Put, for updating user data such as display name, password, email etc...
    }
}
