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
    public class NoteController : ControllerBase
    {
        [HttpGet]
        [Route("notes/{trashed?}")]
        public ActionResult<IEnumerable<NoteDTO>> Get([FromRoute] string trashed)
        {
            try
            {
                string username = HttpContext.User.Identity.Name;
                bool fromTrash = !string.IsNullOrWhiteSpace(trashed);
                UserDataAccess userAccess = new UserDataAccess();
                UserDTO user = userAccess.GetUserByEmailOrUsername(username);
                NoteAccess noteAccess = new NoteAccess();
                IEnumerable<NoteDTO> result = fromTrash 
                    ? noteAccess.GetTrashedByUserId(user.Id)
                    : noteAccess.GetByUserId(user.Id);
                return result.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("Bad data");
            }
        }

        [HttpPost]
        [Route("notes/topicId={topicId}")]
        public ActionResult<APIStateOfNotesInTopic> GetUpdatedByTopicId([FromBody] ClientStateOfNotesInTopic clientData)
        {
            try
            {
                string username = HttpContext.User.Identity.Name;
                UserDataAccess userAccess = new UserDataAccess();
                UserDTO user = userAccess.GetUserByEmailOrUsername(username);

                APIStateOfNotesInTopic apiState = new APIStateOfNotesInTopic();

                TopicAccess topicAccess = new TopicAccess();
                TopicDTO updatedTopic = topicAccess.GetById(clientData.TopicMetadata.Id);

                if (updatedTopic == null)
                {
                    // topic was likely deleted
                    apiState.TopicDeleted = true;
                    return apiState;
                }

                // verify user authorized to read these notes
                else if (!CheckTokenUsernameMatchesUserFromToken(updatedTopic.UserId, username))
                    return Unauthorized();

                // check if client topic is outdated
                if (!updatedTopic.EqualsEntityState(clientData.TopicMetadata))
                    apiState.Topic = updatedTopic;

                NoteAccess noteAccess = new NoteAccess();
                apiState.UpdatedNotes = noteAccess.GetNotesByTopicId(clientData.TopicMetadata.Id).ToList();

                int i = 0;
                while (true && apiState.UpdatedNotes.Count > 0)
                {
                    if (apiState.UpdatedNotes[i].DateDeleted != null)
                        apiState.UpdatedNotes.RemoveAt(i);
                    else i++;
                    if (i == apiState.UpdatedNotes.Count)
                        break;
                }

                foreach (NoteHeaderDTO clientNote in clientData.NoteMetadatas)
                {
                    NoteDTO persistedNote = apiState.UpdatedNotes.FirstOrDefault(n => n.Id == clientNote.Id);
                    if (persistedNote == null)
                        // Note is not existent (anymore) on server --> delete
                        apiState.DeletedNotes.Add(clientNote);
                    else if (persistedNote.EqualsHeader(clientNote))
                        apiState.UpdatedNotes.Remove(persistedNote);
                }
                return apiState;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("Bad data");
            }
        }

        [HttpPost]
        [Route("note")]
        public ActionResult<Guid> Post(NoteDTO note)
        {
            // TODO: Validate item
            // TODO: More accurate return value / http error messages on error

            // Authorize post by matching up collection user id and jwt token identity
            string username = HttpContext.User.Identity.Name;
            if (!CheckTokenUsernameMatchesUserFromToken(note.UserId, username))
                return Unauthorized();

            NoteAccess noteAccess = new NoteAccess();
            try
            {
                Guid id = noteAccess.InsertOrUpdate(note);
                return Created($"api/note/{id}", id);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in NotePostController: {e.GetType()}: {e.Message}");
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("notes")]
        public ActionResult<List<Guid>> Post(List<NoteDTO> notes)
        {
            // TODO: Validate item
            // TODO: More accurate return value / http error messages on error

            if (notes.Count == 0)
                return new List<Guid>();

            // Authorize post by matching up collection user id and jwt token identity
            string username = HttpContext.User.Identity.Name;
            if (!CheckTokenUsernameMatchesUserFromToken(notes[0].UserId, username))
                return Unauthorized();

            NoteAccess noteAccess = new NoteAccess();
            List<Guid> noteIds = new List<Guid>();
            try
            {
                foreach (NoteDTO n in notes)
                {
                    Guid id = noteAccess.InsertOrUpdate(n);
                    noteIds.Add(id);
                }
                return Created($"api/notes", noteIds);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in NotePostController: {e.GetType()}: {e.Message}");
                return BadRequest();
            }
        }

        [Route("note")]
        [HttpPut]
        public ActionResult<Guid> Put([FromBody] NoteDTO note)
        {
            string username = HttpContext.User.Identity.Name;

            NoteAccess noteAccess = new NoteAccess();
            Guid userId = noteAccess.GetUserIdByNoteId(note.Id);

            if (!CheckTokenUsernameMatchesUserFromToken(userId, username))
                return Unauthorized();
            try
            {
                noteAccess.UpdateBody(note);
                return Created($"api/notes/{note.Id}", note.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in NotePutController: {e.GetType()}: {e.Message}");
                return BadRequest();
            }
        }

        [Route("note/meta")]
        [HttpPut]
        public ActionResult<Guid> Put([FromBody] NoteHeaderDTO noteHeader)
        {
            string username = HttpContext.User.Identity.Name;

            NoteAccess noteAccess = new NoteAccess();
            Guid userId = noteAccess.GetUserIdByNoteId(noteHeader.Id);

            if (!CheckTokenUsernameMatchesUserFromToken(userId, username))
                return Unauthorized();
            try
            {
                noteAccess.UpdateHeader(noteHeader);
                return Created($"api/notes/{noteHeader.Id}", noteHeader.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in NotePutController: {e.GetType()}: {e.Message}");
                return BadRequest();
            }
        }

        [Route("note/{id}/topicId={topicId}")]
        [HttpPut]
        public ActionResult<DateTime> Put([FromRoute] Guid id, [FromRoute] Guid topicId)
        {
            string username = HttpContext.User.Identity.Name;

            NoteAccess noteAccess = new NoteAccess();
            Guid userId = noteAccess.GetUserIdByNoteId(id);

            if (!CheckTokenUsernameMatchesUserFromToken(userId, username))
                return Unauthorized();
            try
            {
                DateTime now = DateTime.UtcNow;
                noteAccess.UpdateNoteTopic(id, topicId, now);
                return Ok(now);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in NotePutController: {e.GetType()}: {e.Message}");
                return BadRequest();
            }
        }

        [Route("note/trash/{id}/{urldatetime?}")]
        [HttpPut]
        public ActionResult<DateTime> Put([FromRoute] Guid id, [FromRoute] string urldatetime)
        {
            string username = HttpContext.User.Identity.Name;
            NoteAccess noteAccess = new NoteAccess();
            Guid userId = noteAccess.GetUserIdByNoteId(id);

            if (!CheckTokenUsernameMatchesUserFromToken(userId, username))
                return Unauthorized();

            try
            {
                string dateTime = System.Net.WebUtility.UrlDecode(urldatetime);
                DateTime? dateDeleted = dateTime == null ? null : new DateTime?(DateTime.Parse(dateTime.ToString()).ToUniversalTime());
                DateTime dateModifiedHeader = dateDeleted ?? DateTime.UtcNow;
                noteAccess.TrashUntrash(id, dateDeleted, dateModifiedHeader);
                return Ok(dateModifiedHeader);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in NotePutController: {e.GetType()}: {e.Message}");
                return BadRequest();
            }
        }

        [Route("note/{id}/pin={pinned}")]
        [HttpPut]
        public ActionResult<DateTime> Put([FromRoute] Guid id, [FromRoute] int pinned)
        {
            string username = HttpContext.User.Identity.Name;

            NoteAccess noteAccess = new NoteAccess();
            Guid userId = noteAccess.GetUserIdByNoteId(id);

            if (!CheckTokenUsernameMatchesUserFromToken(userId, username))
                return Unauthorized();
            try
            {
                DateTime now = DateTime.UtcNow;
                noteAccess.TogglePinned(id, pinned != 0, now);
                return Ok(now);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in NotePutController: {e.GetType()}: {e.Message}");
                return BadRequest();
            }
        }

        /// <summary>
        /// This endpoint method has the same signature as Put(id, pinned) thus we cannot name it "Put" as well even though it is a Put method.
        /// So instead, we are calling it post, though ultimately the name is irrelevant.
        /// </summary>
        [Route("note/{id}/globalpin={newvalue}")]
        [HttpPut]
        public ActionResult<DateTime> Post([FromRoute] Guid id, [FromRoute] byte newvalue)
        {
            string username = HttpContext.User.Identity.Name;

            NoteAccess noteAccess = new NoteAccess();
            Guid userId = noteAccess.GetUserIdByNoteId(id);

            if (!CheckTokenUsernameMatchesUserFromToken(userId, username))
                return Unauthorized();
            try
            {
                DateTime now = DateTime.UtcNow;
                noteAccess.ToggleGloballyPinned(id, newvalue != 0, now);
                return Ok(now);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in NotePutController: {e.GetType()}: {e.Message}");
                return BadRequest();
            }
        }

        [Route("note/delete/{id}")]
        [HttpDelete]
        public ActionResult<Guid> Delete([FromRoute] Guid id)
        {
            NoteAccess noteAccess = new NoteAccess();
            Guid userId = noteAccess.GetUserIdByNoteId(id);

            string username = HttpContext.User.Identity.Name;
            if (!CheckTokenUsernameMatchesUserFromToken(userId, username))
                return Unauthorized("Not authorized to delete this resource");
            try
            {
                noteAccess.Delete(id);
                return Ok(id);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in NoteDeleteController: {e.GetType()}: {e.Message}");
                return BadRequest();
            }
        }
    }
}
