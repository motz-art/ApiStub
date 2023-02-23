using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace ApiStub.DataProvider;

public class DataProvider : IDataProvider
{
    private Dictionary<string, List<JsonObject>> data = new();
    IOptionsMonitor<DataProviderOptions> monitor;

    public DataProvider(IOptionsMonitor<DataProviderOptions> monitor)
    {
        this.monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
    }

    public List<JsonObject> GetItems(string name)
    {
        if (data.TryGetValue(name, out var items)) return items;

        var directory = monitor.CurrentValue.DataDirectory;
        var path = Path.Combine(directory ?? "data", name + ".json");
        if (File.Exists(path))
        {
            var fileStream = File.OpenRead(path);
            var jsonArray = (JsonArray)JsonNode.Parse(fileStream);
            if (jsonArray != null)
            {
                items = jsonArray.Select(x => x as JsonObject).Where(x => x != null).ToList()!;
                data.Add(name, items);
                return items;
            }
        }

        items = new List<JsonObject>();
        data.Add(name, items);
        return items;
    }
}