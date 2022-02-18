using Newtonsoft.Json;
using System.Text.Json;

namespace Utils;

public static class JsonUtils {

    public static string Serialize(object obj) =>
        JsonConvert.SerializeObject(obj);

    public static T? Deserialize<T>(string json) =>
        JsonConvert.DeserializeObject<T>(json);

}