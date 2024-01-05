using EazyNotes.Common.JsonConverters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EazyNotes.Models.DTO
{
    public class ClientDTO
    {
        public string DeviceName { get; set; }
        public string Platform { get; set; }

        public ClientDTO() { /* For JSON */ }

        public ClientDTO(string deviceName, string platform)
        {
            DeviceName = deviceName;
            Platform = platform;
        }

        public List<APIValidationError> IsValid()
        {
            if (DeviceName.Length > Constraints.CLIENT_DEVICENAME_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("Client", "DeviceName", "is too long") };
            if (Platform.Length > Constraints.CLIENT_PLATFORM_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("Client", "Platform", "is too long") };
            return null;
        }
    }

    public class Client
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string DeviceName { get; set; }
        public string Platform { get; set; }
        public string Country { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime Registered { get; set; }

        public Client() { /* FOR JSON */ }

        public Client(long id, string username, string devicename, string platform, string country, DateTime registered)
        {
            Id = id;
            Username = username;
            DeviceName = devicename;
            Platform = platform;
            Country = country;
            Registered = registered;
        }
    }
}
