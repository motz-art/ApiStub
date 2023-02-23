using System.Text.Json.Nodes;

namespace ApiStub.DataProvider;

public interface IDataProvider
{
    List<JsonObject> GetItems(string name);
}