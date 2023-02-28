using System.Text.Json.Nodes;
using ApiStub.DataProvider;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using ILogger = Serilog.ILogger;

namespace ApiStub.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiDataController : ControllerBase
    {
        private const string Skip = "skip";
        private const string Limit = "limit";
        private const string OrderBy = "orderBy";
        private const string Id = "id";

        private static readonly ILogger Logger = Log.ForContext<ApiDataController>();

        private readonly IDataProvider dataProvider;

        public ApiDataController(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        [HttpGet("{name:required}/{id:int}")]
        public IActionResult Get(string name, int id)
        {
            Logger.Debug("GET request {name}/{id}", name, id);

            var items = dataProvider.GetItems(name);
            var item = items.FirstOrDefault(x => x[Id]?.GetValue<int>() == id);

            if (item == null) return NotFound();

            return Ok(item);
        }

        [HttpGet("{name:required}")]
        public IActionResult Get(string name, [FromQuery]Dictionary<string, IReadOnlyList<string>> filters)
        {
            Logger.Debug("GET request {name} with filters: {filters}", name, filters);

            var items = dataProvider.GetItems(name) as IEnumerable<JsonObject>;

            foreach (var filter in filters)
            {
                var key = filter.Key;
                if (key.StartsWith("$") || 
                    key.Equals(Limit, StringComparison.OrdinalIgnoreCase) || 
                    key.Equals(Skip, StringComparison.OrdinalIgnoreCase) ||
                    key.Equals(OrderBy, StringComparison.OrdinalIgnoreCase)) continue;

                foreach (var value in filter.Value)
                {
                    items = items.Where(x => x[key]?.ToString()
                        .Equals(value, StringComparison.OrdinalIgnoreCase) ?? false);
                }
            }

            items = items.ToList();

            var totalItems = items.Count();

            if (filters.TryGetValue(OrderBy, out var orderByValues))
            {
                var orderByValue = orderByValues.First();

                var isAscending = orderByValue.Length <= 5 || !orderByValue.EndsWith(" desc");

                var propertyPath = orderByValue;
                if (propertyPath.Contains(' '))
                {
                    propertyPath = propertyPath.Substring(0, propertyPath.IndexOf(' '));
                }

                var propertyPathItems = propertyPath.Split('.');

                if (isAscending)
                {
                    items = items.OrderBy(x => ValueFromPath(x, propertyPathItems), JsonValueComparer.Default);
                }
                else
                {
                    items = items.OrderByDescending(x => ValueFromPath(x, propertyPathItems), JsonValueComparer.Default);
                }
            }

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

        private JsonValue? ValueFromPath(JsonObject? obj, IReadOnlyList<string> propertyPath)
        {
            JsonNode? value = obj;
            foreach (var propName in propertyPath)
            {
                value = value?[propName];
            }

            return value as JsonValue;
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
                    id = items.Max(x => x[Id]?.GetValue<int>());
                }

                id++;

                item[Id] = id;

                items.Add(item);
            }
            else
            {
                item[Id] = id;
                var index = items.FindIndex(x => x[Id]?.GetValue<int>() == id);
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

            items.RemoveAll(x => x[Id]?.GetValue<int>() == id);

            return Ok();
        }

    }
}