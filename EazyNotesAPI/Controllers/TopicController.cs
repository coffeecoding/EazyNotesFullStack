using EazyNotes.Models.DTO;
using EazyNotesAPI.Library.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using static EazyNotesAPI.Internal.TokenUtils;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EazyNotesAPI.Controllers
{
    [Route("api")]
    [Authorize]
    [ApiController]
    public class TopicController : ControllerBase
    {
        // GET: api/<TopicController>
        [HttpGet]
        [Route("topics/{trashed?}")]
        public ActionResult<IEnumerable<TopicDTO>> Get([FromRoute] string trashed)
        {
            try
            {
                string username = HttpContext.User.Identity.Name;
                bool fromTrash = !string.IsNullOrWhiteSpace(trashed);
                UserDataAccess userAccess = new UserDataAccess();
                UserDTO user = userAccess.GetUserByEmailOrUsername(username);
                TopicAccess colAccess = new TopicAccess();
                IEnumerable<TopicDTO> result = fromTrash
                    ? colAccess.GetTrashedByUserId(user.Id)
                    : colAccess.GetByUserId(user.Id);
                return result.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("Bad data");
            }
        }

        [HttpPost]
        [Route("topic")]
        public ActionResult<Guid> Post(TopicDTO topic)
        {
            // TODO: Validate topic
            // TODO: More accurate return value / http error messages on error
            // Authorize post by matching up topic user id and jwt token identity
            string username = HttpContext.User.Identity.Name;
            
            if (!CheckTokenUsernameMatchesUserFromToken(topic.UserId, username))
                return Unauthorized();

            TopicAccess colAccess = new TopicAccess();
            try
            {
                Guid id = colAccess.InsertOrUpdate(topic);
                return Created($"api/topic/{id}", id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("topics")]
        public ActionResult<List<Guid>> Post(List<TopicDTO> topics)
        {
            // TODO: Validate topic
            // TODO: More accurate return value / http error messages on error
            // Authorize post by matching up topic user id and jwt token identity
            string username = HttpContext.User.Identity.Name;

            if (topics.Count == 0)
                return new List<Guid>();

            if (!CheckTokenUsernameMatchesUserFromToken(topics[0].UserId, username))
                return Unauthorized();

            TopicAccess colAccess = new TopicAccess();
            List<Guid> topicIds = new List<Guid>();
            try
            {
                foreach (TopicDTO t in topics)
                {
                    Guid id = colAccess.InsertOrUpdate(t);
                    topicIds.Add(id);
                }
                return Created($"api/topics", topicIds);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest();
            }
        }

        [Route("topic")]
        [HttpPut]
        public ActionResult<Guid> Put(TopicDTO topic)
        {
            // TODO: Validate topic
            // TODO: More accurate return value / http error messages on error

            // Authorize post by matching up topic user id and jwt token identity
            TopicAccess topicAccess = new TopicAccess();
            Guid userId = topicAccess.GetUserIdByTopicId(topic.Id);

            string username = HttpContext.User.Identity.Name;
            if (!CheckTokenUsernameMatchesUserFromToken(userId, username))
                return Unauthorized();
            try
            {
                topicAccess.UpdateBody(topic);
                return Created($"api/topics/{topic.Id}", topic.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest();
            }
        }

        [Route("topics/positions")]
        [HttpPut] 
        public ActionResult<DateTime> Put([FromBody] TopicPositionData topicPositionData)
        {
            int topicCount = topicPositionData.TopicIds.Count;
            if (topicPositionData.TopicPositions.Count != topicCount)
                return null;
            if (topicCount == 0)
                return null;

            try
            {
                TopicAccess topicAccess = new TopicAccess();
                Guid userIdFromJWT = Guid.Parse(HttpContext.User.Claims.SingleOrDefault(c => c.Type == JWT_TYPE_NAMEIDENTIFIER).Value);
                List<TopicDTO> topics = topicAccess.GetByUserId(userIdFromJWT).ToList();
                DateTime now = DateTime.UtcNow;

                for (int i=0; i<topicCount; i++)
                {
                    TopicDTO topic = topics.SingleOrDefault(t => t.Id == topicPositionData.TopicIds[i]);
                    if (topic == null)
                        continue;
                    topicAccess.UpdatePosition(topicPositionData.TopicIds[i], topicPositionData.TopicPositions[i], now);
                }

                return Ok(now);
            }
            catch (Exception e)
            {
                string error = $"Error in PutController for topic positions: {e.GetType()} {e.Message}";
                Console.WriteLine(error);
                return BadRequest();
            }
        }

        [Route("topic/meta")]
        [HttpPut]
        public ActionResult<Guid> Put(TopicHeaderDTO topicHeader)
        {
            // TODO: Validate topic
            // TODO: More accurate return value / http error messages on error

            // Authorize post by matching up topic user id and jwt token identity
            TopicAccess topicAccess = new TopicAccess();
            Guid userId = topicAccess.GetUserIdByTopicId(topicHeader.Id);

            string username = HttpContext.User.Identity.Name;
            if (!CheckTokenUsernameMatchesUserFromToken(userId, username))
                return Unauthorized();
            try
            {
                topicAccess.UpdateHeader(topicHeader);
                return Created($"api/topics/{topicHeader.Id}", topicHeader.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest();
            }
        }

        [Route("topic/trash/{id}/{urldatetime?}")]
        [HttpPut]
        public ActionResult<DateTime> Put([FromRoute] Guid id, [FromRoute] string urldatetime)
        {
            string username = HttpContext.User.Identity.Name;
            TopicAccess topicAccess = new TopicAccess();
            Guid userId = topicAccess.GetUserIdByTopicId(id);

            if (!CheckTokenUsernameMatchesUserFromToken(userId, username))
                return Unauthorized();

            try
            {
                string dateTime = System.Net.WebUtility.UrlDecode(urldatetime);
                DateTime? dateDeleted = dateTime == null ? null : new DateTime?(DateTime.Parse(dateTime.ToString()).ToUniversalTime());
                DateTime dateModifiedHeader = dateDeleted ?? DateTime.UtcNow;
                topicAccess.TrashUntrash(id, dateDeleted, dateModifiedHeader);
                return Ok(dateModifiedHeader);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in TopicPutController toggletrash: {e.GetType()}: {e.Message}");
                return BadRequest();
            }
        }

        [Route("topic/delete/{id}")]
        [HttpDelete]
        public ActionResult<Guid> Delete([FromRoute] Guid id)
        {
            try
            {
                TopicAccess colAccess = new TopicAccess();
                Guid topicUserId = colAccess.GetUserIdByTopicId(id);

                string username = HttpContext.User.Identity.Name;
                if (!CheckTokenUsernameMatchesUserFromToken(topicUserId, username))
                    return Unauthorized("Not authorized to delete this resource");

                colAccess.Delete(id);
                return Ok(id);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in TopicDeleteController: {e.GetType()}: {e.Message}");
                return BadRequest();
            }
        }
    }
}
