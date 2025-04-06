using System.Text.Json;

namespace AppEngine.Json;

public class Serializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public string Serialize<T>(T x)
    {
        return System.Text.Json.JsonSerializer.Serialize(x, SerializerOptions);
    }


    public T? Deserialize<T>(string json)
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }
}