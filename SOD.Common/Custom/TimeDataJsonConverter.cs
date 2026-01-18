using SOD.Common.Helpers;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOD.Common.Custom
{
    /// <summary>
    /// A json converter to help serialize the timedata structure of Lib.Time
    /// </summary>
    public sealed class TimeDataJsonConverter : JsonConverter<Time.TimeData>
    {
        public override Time.TimeData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var timeString = reader.GetString();
                return Time.TimeData.Deserialize(timeString);
            }
            throw new JsonException("Expected a string for TimeData.");
        }

        public override void Write(Utf8JsonWriter writer, Time.TimeData value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Serialize());
        }
    }
}
