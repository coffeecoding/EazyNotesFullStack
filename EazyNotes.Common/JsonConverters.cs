using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EazyNotes.Common.JsonConverters
{
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        public DateTime ConvertBack(string date)
        {
            DateTime prevalue = DateTime.Parse(date);
            DateTime value = prevalue.ToUniversalTime();
            DateTime result = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            return result;
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ConvertBack(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToISO8601UTCString());
        }
    }

    public class CustomDateTimeNullableConverter : JsonConverter<DateTime?>
    {
        public DateTime? ConvertBack(string date)
        {
            if (string.IsNullOrWhiteSpace(date))
                return null;
            return DateTime.SpecifyKind(DateTime.Parse(date).ToUniversalTime(), DateTimeKind.Utc);
        }

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ConvertBack(reader.GetString());
        }

        public string Convert(DateTime? dateTime)
        {
            return dateTime.ToISO8601UTCString();
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Convert(value));
        }
    }
}
