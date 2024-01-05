using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace EazyNotesAPI.Hubs
{
    [Authorize]
    public class DataHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var username = Context.User.Identity.Name;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{username}");
            await base.OnConnectedAsync();
            Console.WriteLine($"SignalR DataHub: Connected user {username}");
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.Identity.Name;
            Console.WriteLine($"SignalR DataHub: Disconnected user {username}");
            Console.WriteLine($"SignalR DataHub: OnDisconnected Async {exception}");
            return base.OnDisconnectedAsync(exception);
        }

        /*** Entity Methods ***/
        public async Task SyncEntityTrashed(long id, bool isTopic, string dateTrashed)
        {
            var username = Context.User.Identity.Name;
            await Clients.OthersInGroup($"user_{username}").SendAsync("AcceptEntityTrashed", id, isTopic, dateTrashed);
        }

        public async Task SyncEntityDelete(long id, bool isTopic)
        {
            var username = Context.User.Identity.Name;
            await Clients.OthersInGroup($"user_{username}").SendAsync("AcceptEntityDelete", id, isTopic);
        }


        /***  Note Methods ***/
        public async Task SyncNoteInsert(string noteDTO)
        {
            var username = Context.User.Identity.Name;
            await Clients.OthersInGroup($"user_{username}").SendAsync("AcceptSyncNoteInsert", noteDTO);
        }

        public async Task SyncNoteUpdateBody(string noteBody)
        {
            var username = Context.User.Identity.Name;
            await Clients.OthersInGroup($"user_{username}").SendAsync("AcceptSyncNoteUpdateBody", noteBody);
        }

        public async Task SyncNoteUpdateBodyMetadata(string noteBodyMeta)
        {
            var username = Context.User.Identity.Name;
            await Clients.OthersInGroup($"user_{username}").SendAsync("AcceptSyncNoteUpdateBodyMetadata", noteBodyMeta);
        }

        public async Task SyncNoteTopic(long noteId, long newTopicId)
        {
            var username = Context.User.Identity.Name;
            await Clients.OthersInGroup($"user_{username}").SendAsync("AcceptSyncNoteTopic", noteId, newTopicId);
        }

        public async Task SyncNotePinned(long noteId, bool newPinnedValue)
        {
            var username = Context.User.Identity.Name;
            await Clients.OthersInGroup($"user_{username}").SendAsync("AcceptSyncNotePinned", noteId, newPinnedValue);
        }

        public async Task SyncNoteOptions(long noteId, string newOptions)
        {
            var username = Context.User.Identity.Name;
            await Clients.OthersInGroup($"user_{username}").SendAsync("AcceptSyncNoteOptions", noteId, newOptions);
        }


        /***  Topic Methods ***/
        public async Task SyncTopicInsert(string updatedTopicJson)
        {
            var username = Context.User.Identity.Name;
            await Clients.OthersInGroup($"user_{username}").SendAsync("AcceptSyncTopicInsert", updatedTopicJson);
        }

        public async Task SyncTopicUpdateBody(string updatedTopicBody)
        {
            var username = Context.User.Identity.Name;
            await Clients.OthersInGroup($"user_{username}").SendAsync("AcceptSyncTopicUpdateBody", updatedTopicBody);
        }

        public async Task SyncTopicUpdateBodyMetadata(string updatedTopicMeta)
        {
            var username = Context.User.Identity.Name;
            await Clients.OthersInGroup($"user_{username}").SendAsync("AcceptSyncTopicUpdateBodyMetadata", updatedTopicMeta);
        }

        public async Task SyncTopicSymbol(long topicId, string newSymbol)
        {
            var username = Context.User.Identity.Name;
            await Clients.OthersInGroup($"user_{username}").SendAsync("AcceptSyncTopicSymbol", topicId, newSymbol);
        }
    
        public async Task SyncTopicColor(long topicId, string newColor)
        {
            var username = Context.User.Identity.Name;
            await Clients.OthersInGroup($"user_{username}").SendAsync("AcceptSyncTopicColor", topicId, newColor);
        }

        public async Task SyncTopicPosition(long topicId, string newPosition)
        {
            var username = Context.User.Identity.Name;
            await Clients.OthersInGroup($"user_{username}").SendAsync("AcceptSyncTopicPosition", topicId, newPosition);
        }
    }
}
