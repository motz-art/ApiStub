using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ApiStub.Controllers
{
    public interface IDataProvider
    {
        List<JsonObject> GetItems(string name);
    }

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

    [ApiController]
    [Route("api")]
    public class ApiDataController : ControllerBase
    {
        private const string Skip = "skip";
        private const string Limit = "limit";

        private static ILogger _logger = Log.ForContext<ApiDataController>();

        private IDataProvider dataProvider;

        public ApiDataController(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        [HttpGet("{name:required}")]
        public IActionResult Get(string name, [FromQuery]Dictionary<string, IReadOnlyList<string>> filters)
        {
            var items = dataProvider.GetItems(name) as IEnumerable<JsonObject>;

            foreach (var filter in filters)
            {
                var key = filter.Key;
                if (key.StartsWith("$") || 
                    key.Equals(Limit, StringComparison.OrdinalIgnoreCase) || 
                    key.Equals(Skip, StringComparison.OrdinalIgnoreCase)) continue;

                foreach (var value in filter.Value)
                {
                    items = items.Where(x => x[key]?.ToString()
                        .Equals(value, StringComparison.OrdinalIgnoreCase) ?? false);
                }
            }

            items = items.ToList();

            var totalItems = items.Count();

            if (filters.TryGetValue(Skip, out var skipValues))
            {
                items = items.Skip(int.Parse(skipValues.First()));
            }

            if (filters.TryGetValue(Limit, out var limitValues))
            {
                items = items.Take(int.Parse(limitValues.First()));
            }

            return Ok(new
            {
                items,
                totalItems,
            });
        }

        [HttpPut("{name:required}/{id:int}")]
        public IActionResult Save(string name, int id, [FromBody] JsonObject item)
        {
            return SaveItem(name, item, id);
        }

        [HttpPost("{name:required}")]
        public IActionResult Save(string name, [FromBody] JsonObject item)
        {
            return SaveItem(name, item);
        }

        private IActionResult SaveItem(string name, JsonObject item, int? id = null)
        {
            var items = dataProvider.GetItems(name);

            if (id == null)
            {
                id = 0;
                if (items.Any())
                {
                    id = items.Max(x => x["id"]?.GetValue<int>());
                }

                id++;

                item["id"] = id;

                items.Add(item);
            }
            else
            {
                item["id"] = id;
                var index = items.FindIndex(x => x["id"]?.GetValue<int>() == id);
                if (index < 0)
                {
                    return NotFound($"Item with Id {id} is not found.");
                }
                items[index] = item;
            }

            return Ok(item);
        }

        [HttpDelete("{name:required}/{id:int}")]
        public IActionResult Delete(string name, int id)
        {
            var items = dataProvider.GetItems(name);

            items.RemoveAll(x => x["id"]?.GetValue<int>() == id);

            return Ok();
        }

    }
}