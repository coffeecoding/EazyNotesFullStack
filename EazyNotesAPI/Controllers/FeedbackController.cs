using EazyNotes.Models.DTO;
using EazyNotesAPI.Library.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EazyNotesAPI.Controllers
{
    [Route("api/feedback")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<FeedbackDTO>> Get()
        {
            try
            {
                string username = HttpContext.User.Identity.Name;
                if (!username.Equals("dev"))
                    return Unauthorized();
                FeedbackAccess fa = new FeedbackAccess();
                return fa.Get().ToList(); 
            } catch (Exception e)
            {
                Console.WriteLine($"Exception in FeedbackController.Get: {e.GetType()} {e.Message}");
                return BadRequest();
            }
        }

        [HttpPost]
        public ActionResult<long> Post([FromBody] FeedbackDTO feedback)
        {
            try
            {
                FeedbackAccess fa = new FeedbackAccess();
                long id = fa.Insert(feedback);
                return Created($"api/feedback/{id}", id);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in FeedbackController.Post: {e.GetType()} {e.Message}");
                return BadRequest();
            }
        }

        [Authorize]
        [Route("{id}/addressed={addressed}")]
        [HttpPut]
        public ActionResult Update([FromRoute] long id, [FromRoute] int addressed)
        {
            try
            {
                string username = HttpContext.User.Identity.Name;
                if (!username.Equals("dev"))
                    return Unauthorized();

                bool newVal = addressed != 0;

                FeedbackAccess fa = new FeedbackAccess();
                fa.SetAddressed(id, newVal);
                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in FeedbackController.Update: {e.GetType()} {e.Message}");
                return BadRequest();
            }
        }

        [Authorize]
        [Route("{id}")]
        [HttpDelete]
        public ActionResult<long> Delete([FromRoute] long id)
        {
            try
            {
                string username = HttpContext.User.Identity.Name;
                if (!username.Equals("dev"))
                    return Unauthorized();
                FeedbackAccess fa = new FeedbackAccess();
                fa.Delete(id);
                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in FeedbackController.Delete: {e.GetType()} {e.Message}");
                return BadRequest();
            }
        }
    }
}
