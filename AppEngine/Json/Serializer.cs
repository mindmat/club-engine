using System.Text.Json;

namespace AppEngine.Json;

public class Serializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public string Serialize<T>(T x)
    {
        return JsonSerializer.Serialize(x, SerializerOptions);
    }


    public T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }
}