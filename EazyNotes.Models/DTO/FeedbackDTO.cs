using EazyNotes.Common.JsonConverters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EazyNotes.Models.DTO
{
    public enum FeedbackCategory
    {
        Bug, 
        FeatureRequest,
        Other
    }

    public class FeedbackDTO
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public Int16 Category { get; set; }
        public string AppVersion { get; set; }
        public string AddressedInVersion { get; set; }
        public string DeviceName { get; set; }
        public string Platform { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime Submitted { get; set; }

        public FeedbackDTO()
        {
        }

        public List<APIValidationError> IsValid()
        {
            if (Title.Length > Constraints.FEEDBACK_TITLE_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("Feedback", "Title", "is too long") };
            if (Body.Length > Constraints.FEEDBACK_BODY_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("Feedback", "Body", "is too long") };
            if (Category < 0 || Category > Constraints.FEEDBACK_CATEGORY_MAX)
                return new List<APIValidationError>() { new APIValidationError("Feedback", "Category", "is invalid") };
            if (AppVersion.Length > Constraints.FEEDBACK_APPVERSION_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("Feedback", "AppVersion", "is too long") };
            if (AddressedInVersion.Length > Constraints.FEEDBACK_ADDRESSEDINVERSION_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("Feedback", "AddressedInVersion", "is too long") };
            if (DeviceName.Length > Constraints.FEEDBACK_DEVICENAME_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("Feedback", "DeviceName", "is too long") };
            if (Platform.Length > Constraints.FEEDBACK_PLATFORM_MAXLEN)
                return new List<APIValidationError>() { new APIValidationError("Feedback", "Platform", "is too long") };
            return null;
        }

        public FeedbackDTO(long id, string title, string body, Int16 category, 
            string appVer, string deviceName, string platform, DateTime submitted) 
        {
            Id = id;
            Title = title;
            Body = body;
            Category = category;
            AppVersion = appVer;
            DeviceName = deviceName;
            Platform = platform;
            Submitted = submitted;
        }
    }
}
