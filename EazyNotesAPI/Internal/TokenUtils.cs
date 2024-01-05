using EazyNotesAPI.Library.DataAccess;
using EazyNotes.CryptoServices;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EazyNotes.Models.DTO;

namespace EazyNotesAPI.Internal
{
    public class TokenUtils
    {
        public static string JWT_TYPE_NAMEIDENTIFIER = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        public static bool CheckTokenUsernameMatchesUserFromToken(Guid id, string username)
        {
            UserDataAccess userDataAccess = new UserDataAccess();
            UserDTO user = userDataAccess.GetUserByEmailOrUsername(username);
            return user.Id.Equals(id);
        }

        public static string GetUsernameByEmail(string email)
        {
            UserDataAccess userDataAccess = new UserDataAccess();
            UserDTO user = userDataAccess.GetUserByEmailOrUsername(email);
            return user.Username;
        }

        public static bool IsValidCredentials(string emailOrUsername, string pwHash)
        {
            UserDataAccess userData = new UserDataAccess();
            UserDTO user = userData.GetUserByEmailOrUsername(emailOrUsername);
            if (user == null)
                return false;

            string rehashedClaimedPassword = HashingHelper.PerformHash(pwHash, HashingHelper.CURRENT_VERSION);

            return user.PasswordHash.Equals(rehashedClaimedPassword);
        }

        public static dynamic GenerateToken(string username, Guid userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.Now.AddDays(7)).ToUnixTimeSeconds().ToString()),
            };

            var token = new JwtSecurityToken(
                new JwtHeader(
                    new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySuperSecretKeyWhichIsActuallySecret")),
                        SecurityAlgorithms.HmacSha256)),
                new JwtPayload(claims));

            // here comes the dynamic object ! (anonymous class object)
            var output = new
            {
                Access_Token = new JwtSecurityTokenHandler().WriteToken(token),
                Username = username
            };

            return output;
        }
    }
}
