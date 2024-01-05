using EazyNotes.Common;
using EazyNotes.Common.JsonConverters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EazyNotes.Models.DTO
{
    public class TopicPositionData
    {
        public List<Guid> TopicIds { get; set; }
        public List<int> TopicPositions { get; set; }
        public DateTime TimeStamp { get; set; }

        public TopicPositionData()
        {
            TopicIds = new List<Guid>();
            TopicPositions = new List<int>();
            TimeStamp = DateTime.UtcNow;
        }

        public TopicPositionData(List<Guid> topicIds, List<int> topicPositions, DateTime timestamp)
        {
            TopicIds = topicIds;
            TopicPositions = topicPositions;
            TimeStamp = timestamp;
        }
    }

    public class TopicHeaderDTO : EntityDTO, IEquatable<TopicHeaderDTO>
    {
        public string Symbol { get; set; }
        public string Color { get; set; }
        public int Position { get; set; }

        public TopicHeaderDTO()
        {

        }

        public TopicHeaderDTO(Guid id, string symbol = "", string color = "",
            int position = 0, DateTime? dateDeleted = null)
        {
            Id = id;
            Symbol = symbol;
            Color = color;
            Position = position;
            DateDeleted = dateDeleted;
        }

        public TopicHeaderDTO(Guid id, string symbol, string color,
            int position, DateTime? dateDeleted, DateTime dateCreated, DateTime dateModifiedHeader, DateTime dateModified)
        {
            Id = id;
            Symbol = symbol;
            Color = color;
            Position = position;
            DateDeleted = dateDeleted;
            DateCreated = dateCreated;
            DateModifiedHeader = dateModifiedHeader;
            DateModified = dateModified;
        }

        public TopicHeaderDTO(Guid id, DateTime? dateDeleted)
        {
            Id = id;
            DateDeleted = dateDeleted;
        }

        /// <summary>
        /// Checks if the metadata of the topics is equal, that is, Id, Symbol, Color, DateDeleted and DateLastEdited.
        /// </summary>
        public bool EqualsHeader([AllowNull] TopicHeaderDTO other)
        {
            if (other == null)
                return false;
            bool c0 = base.EqualsHeader(other);
            bool c1 = Symbol == other.Symbol && Color == other.Color && Position == other.Position;
            return c0 && c1;
        }

        /// <summary>
        /// Checks if the contents of the topics are equal, that is, Id and DateLastEdited.
        /// </summary>
        public bool EqualsBody([AllowNull] TopicHeaderDTO other)
        {
            if (other == null)
                return false;
            return Id == other.Id && DateModified.IsEqual(other.DateModified);
        }

        public bool Equals([AllowNull] TopicHeaderDTO other)
        {
            if (other == null)
                return false;
            return EqualsBody(other) && EqualsHeader(other);
        }
    }
}
