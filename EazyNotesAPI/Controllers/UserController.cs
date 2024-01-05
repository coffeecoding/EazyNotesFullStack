using EazyNotes.CryptoServices;
using EazyNotes.Models.DTO;
using EazyNotes.Models.POCO;
using EazyNotesAPI.Library.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace EazyNotesAPI.Controllers
{
    [Authorize]
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [Route("user")]
        [HttpGet]
        //[Consumes("application/json")]
        public ActionResult<User> GetById()
        {
            // TODO: More specific error handling, validation and return messages!
            try
            {
                string claimedUsername = HttpContext.User.Identity.Name;
                UserDataAccess userDataAccess = new UserDataAccess();
                UserDTO user = userDataAccess.GetUserByEmailOrUsername(claimedUsername);
                return user.ToUser();
            } catch (Exception)
            {
                return BadRequest("Failed to grab data");
            }
        }

        [Route("user/{id?}")]
        [HttpPut]
        public ActionResult Put(UserDTO user)
        {
            UserDataAccess userDataAccess = new UserDataAccess();

            // Check if User already exists, return error if so
            UserDTO existingUser = userDataAccess.GetUserById(user.Id.ToString());
            if (existingUser == null)
            {
                // This is catching the UNLIKELY event that an existing Guid was seeded again.
                return BadRequest("Bad Request.");
            }
            existingUser = userDataAccess.GetUserByEmailOrUsername(user.Username);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return BadRequest("User with this username already exists.");
            }
            existingUser = userDataAccess.GetUserByEmailOrUsername(user.Email);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return BadRequest("User with this email address already exists.");
            }
            if (!EazyNotes.Models.POCO.User.IsValidNamesAndEmail(user.Username, user.Email, user.DisplayName))
            {
                return BadRequest("Invalid input.");
            }

            try {
                user.PasswordHash = HashingHelper.PerformHash(user.PasswordHash, HashingHelper.CURRENT_VERSION);
                userDataAccess.UpdateUser(user);
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("Bad data");
            }

            return Ok();
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
