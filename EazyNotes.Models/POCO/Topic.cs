using EazyNotes.Common;
using EazyNotes.Models.DTO;
using System;

namespace EazyNotes.Models.POCO
{
    public class Topic : Entity, ICloneable
    {
        public string Color { get; set; }
        public int Position { get; set; }

        public Topic()
        {

        }

        public Topic(string title, string symbol)
        {
            Title = title;
            Symbol = symbol;
        }

        public Topic(Guid userId, string title = "", string symbol = "", string color = "")
        {
            UserId = userId;
            Title = title;
            Symbol = symbol.Equals("") ? Symbols.DefaultTopicSymbol : symbol;
            Color = color.Equals("") ? ColorRef.DefaultTopicColor : color;

            Position = 0;
            DateTime now = DateTime.UtcNow;
            DateCreated = now;
            DateModifiedHeader = now;
            DateModified = now;
        }

        public Topic(Guid userId, string symbol, string color)
        {
            UserId = userId;
            Title = "";
            Symbol = symbol;
            Color = color;

            Position = 0;
            DateTime now = DateTime.UtcNow;
            DateCreated = now;
            DateModified = now;
        }

        public Topic(Guid id, Guid userId, string title, string symbol)
        {
            Id = id;
            UserId = userId;
            Title = title;
            Symbol = symbol;
            Color = ColorRef.DefaultTopicColor;
            Position = 0;
        }

        public Topic(Guid id, Guid userId, string title, string symbol, DateTime created, 
            DateTime dateModifiedHeader, DateTime dateModified, DateTime? deleted, string ivkey, string color, int position)
        {
            Id = id;
            UserId = userId;
            Title = title;
            Symbol = symbol;
            DateCreated = created;
            DateModifiedHeader = dateModifiedHeader;
            DateModified = dateModified;
            DateDeleted = deleted;
            IVKey = ivkey;
            Color = color;
            Position = position;
        }

        public Topic(TopicDTO topicDTO)
        {
            Id = topicDTO.Id;
            UserId = topicDTO.UserId;
            Title = topicDTO.Title;
            Symbol = topicDTO.Symbol;
            DateCreated = topicDTO.DateCreated;
            DateModifiedHeader = topicDTO.DateModifiedHeader;
            DateModified = topicDTO.DateModified;
            DateDeleted = topicDTO.DateDeleted;
            IVKey = topicDTO.IVKey;
            Color = topicDTO.Color;
            Position = topicDTO.Position;
        }

        public void UpdateHeaderFrom(TopicHeaderDTO other)
        {
            Symbol = other.Symbol;
            DateModified = other.DateModified;
            DateModifiedHeader = other.DateModifiedHeader;
            DateDeleted = other.DateDeleted;
            Color = other.Color;
            Position = other.Position;
        }

        public void UpdateFrom(Topic other)
        {
            Id = other.Id;
            Title = other.Title;
            Symbol = other.Symbol;
            DateModified = other.DateModified;
            DateModifiedHeader = other.DateModifiedHeader;
            DateDeleted = other.DateDeleted;
            Color = other.Color;
            Position = other.Position;
        }

        /// <summary>
        /// Returns true if the encrypted parts of the body are equal, that is the just the title.
        /// </summary>
        public override bool BodyEquals(Entity other)
        {
            if (other == null)
                return false;
            return Id.Equals(other.Id) && DateModified.IsEqual(other.DateModified);
        }

        /// <summary>
        /// Returns true if BodyEquals && BodyMetadataEquals both return true.
        /// </summary>
        public override bool Equals(object obj)
        {
            Entity entity = obj as Entity;
            return BodyEquals(entity) && BodyMetadataEquals(entity);
        }

        public void RemoveKeyAndDataFields()
        {
            Id = Guid.Empty;
            UserId = Guid.Empty;
            Title = IVKey = "";
        }

        public object Clone()
        {
            return new Topic(Id, UserId, Title, Symbol, DateCreated, DateModifiedHeader, DateModified, DateDeleted, IVKey, Color, Position);
        }

        public TopicDTO ToTopicDTO()
        {
            return new TopicDTO(Id, UserId, Title, Symbol, DateCreated, DateModifiedHeader, DateModified, DateDeleted, IVKey, Color, Position);
        }

        public TopicHeaderDTO GetHeader()
        {
            return new TopicHeaderDTO(Id, Symbol, Color, Position, DateDeleted, DateCreated, DateModifiedHeader, DateModified);
        }

        public override EntitySyncState ToEntitySyncState(DateTime? date, bool setDeleteFlag = false)
            => new EntitySyncState(Id, true, date, setDeleteFlag);
    }
}
