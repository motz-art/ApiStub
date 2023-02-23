using System.Text.Json;
using System.Text.Json.Nodes;

namespace ApiStub.Controllers;

public class JsonValueComparer : IComparer<JsonValue?>
{
    private static JsonValueComparer? _default;
    public static JsonValueComparer Default => _default ??= new JsonValueComparer();


    public int Compare(JsonValue? x, JsonValue? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var xKind = x.GetValue<JsonElement>().ValueKind;
        var yKind = y.GetValue<JsonElement>().ValueKind;

        if (xKind == JsonValueKind.Number && yKind == JsonValueKind.Number)
        {
            return Comparer<decimal>.Default.Compare(x.GetValue<decimal>(), y.GetValue<decimal>());
        }

        return StringComparer.OrdinalIgnoreCase.Compare(x.ToString(), y.ToString());
    }
}