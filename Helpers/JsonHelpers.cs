using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chowbi_Utilities.Helpers
{
    public class JsonTimespanConverter : JsonConverter<TimeSpan?>
    {
        public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (TimeSpan.TryParse(reader.GetString(), out TimeSpan result))
                return result;
            else
                return null;

        }

        public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value?.ToString());
        }
    }
}
