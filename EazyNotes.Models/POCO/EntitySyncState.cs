using EazyNotes.Common.JsonConverters;
using System;
using System.Text.Json.Serialization;

namespace EazyNotes.Models.POCO
{
    public class EntitySyncState
    {
        public Guid Id { get; set; }
        public bool IsTopic { get; set; }
        [JsonConverter(typeof(CustomDateTimeNullableConverter))]
        public DateTime? DateSynced { get; set; }
        public bool Delete { get; set; }

        public EntitySyncState() { }
        public EntitySyncState(Guid id, bool isTopic, DateTime? dateSynced, bool delete)
        {
            Id = id; IsTopic = isTopic; DateSynced = dateSynced; Delete = delete;
        }
    }
}