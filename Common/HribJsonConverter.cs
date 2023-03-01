using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Kafe;

public class HribJsonConverter : JsonConverter<Hrib>
{
    public override Hrib? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (Hrib?)reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, Hrib value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
