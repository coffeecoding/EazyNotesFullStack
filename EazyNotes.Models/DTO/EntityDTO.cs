using EazyNotes.Common;
using EazyNotes.Common.JsonConverters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EazyNotes.Models.DTO
{
    public class EntityDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime DateCreated { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime DateModifiedHeader { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime DateModified { get; set; }
        [JsonConverter(typeof(CustomDateTimeNullableConverter))]
        public DateTime? DateDeleted { get; set; }

        public EntityDTO() { /* for Json*/ }
        public EntityDTO(Guid id) { Id = id; }
        public EntityDTO(Guid id, Guid userId, DateTime dateCreated,
            DateTime dateModifiedHeader, DateTime dateModified, DateTime? dateDeleted) 
        {
            Id = id;
            UserId = userId;
            DateCreated = dateCreated;
            DateModifiedHeader = dateModifiedHeader;
            DateModified = dateModified;
            DateDeleted = dateDeleted;
        }

        public bool IsPersistedLocally() => !Id.Equals(Guid.Empty);

        public virtual List<APIValidationError> IsValid() { return null; }

        public virtual bool EqualsHeader(EntityDTO other)
        {
            bool c1 = Id.Equals(other.Id);
            bool c2 = DateDeleted.IsEqual(other.DateDeleted);
            bool c3 = DateModified.IsEqual(other.DateModified);
            bool c4 = DateModifiedHeader.IsEqual(other.DateModifiedHeader);
            return c1 && c2 && c3 && c4;
        }

        public bool EqualsEntityState(EntityDTO other)
        {
            bool c1 = Id == other.Id;
            bool c2 = DateDeleted.IsEqual(other.DateDeleted);
            bool c3 = DateModifiedHeader.IsEqual(other.DateModifiedHeader);
            return c1 && c2 && c3;
        }
    }
}
