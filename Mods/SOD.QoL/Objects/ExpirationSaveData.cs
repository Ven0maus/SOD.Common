using SOD.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOD.QoL.Objects
{
    internal class ExpirationSaveData
    {
        public Dictionary<string, Time.TimeData> Expirations { get; set; }

        public string Serialize()
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new TimeDataJsonConverter() },
                WriteIndented = true
            };

            return JsonSerializer.Serialize(this, options);
        }

        public static ExpirationSaveData Deserialize(string json)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new TimeDataJsonConverter() },
                WriteIndented = true
            };

            return JsonSerializer.Deserialize<ExpirationSaveData>(json, options);
        }
    }

    internal class TimeDataJsonConverter : JsonConverter<Time.TimeData>
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
