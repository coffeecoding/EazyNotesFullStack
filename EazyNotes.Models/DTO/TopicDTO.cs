using Dapper.Contrib.Extensions;
using EazyNotes.Common;
using EazyNotes.Models.POCO;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EazyNotes.Models.DTO
{
    [Table("Topics")]
    public class TopicDTO : TopicHeaderDTO, IEquatable<TopicDTO>
    {
        public string Title { get; set; }
        public string IVKey { get; set; }

        public TopicDTO()
        {
            // empty ctor needed by dapper contrib
        }

        public TopicDTO(Guid id, Guid userId, string title, string symbol, DateTime created,
            DateTime dateModifiedHeader, DateTime dateModified, DateTime? dateDeleted, string ivkey, string color, int position)
            : base(id, symbol, color, position, dateDeleted, created, dateModifiedHeader, dateModified)
        {
            UserId = userId;
            Title = title;
            IVKey = ivkey;
        }

        public override List<APIValidationError> IsValid()
        {
            if (Id == Guid.Empty)
                return new List<APIValidationError>() { new APIValidationError("Topic", "Id", "may not be empty") };
            if (UserId == Guid.Empty)
                return new List<APIValidationError>() { new APIValidationError("Topic", "UserId", "may not be empty") };
            if (Title.Length >= Constraints.TOPIC_TITLE_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("Topic", "Title", "is too long") };
            if (Position < 0)
                return new List<APIValidationError>() { new APIValidationError("Topic", "Position", "may not be negative") };
            if (Position > Constraints.TOPIC_POSITION_MAX)
                return new List<APIValidationError>() { new APIValidationError("Topic", "Position", "is too large") };
            if (Symbol.Length > Constraints.TOPIC_SYMBOL_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("Topic", "Symbol", "is too long") };
            if (Color.Length > Constraints.TOPIC_COLOR_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("Topic", "Options", "is too long") };
            if (IVKey.Length > Constraints.TOPIC_IVKEY_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("Topic", "IVKey", "is too long") };
            return null;
        }

        public Topic ToTopic()
        {
            return new Topic(this);
        }

        public override string ToString()
        {
            return "Topic: " + Title;
        }

        public bool Equals([AllowNull] TopicDTO other)
        {
            return Id == other.Id && UserId == other.UserId && Symbol == other.Symbol && Title == other.Title &&
                Position == other.Position && Color == other.Color && IVKey == other.IVKey &&
                DateDeleted.IsEqual(other.DateDeleted) && DateCreated.IsEqual(other.DateCreated) &&
                DateModifiedHeader.IsEqual(other.DateModifiedHeader) && DateModified.IsEqual(other.DateModified);
        }
    }
}
