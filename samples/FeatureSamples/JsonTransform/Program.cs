using Microsoft.AzureHealth.DataServices.Json.Transforms;
using Newtonsoft.Json.Linq;

#pragma warning disable CA1852

internal class Program
{
    private static async Task Main(string[] args)
    {
        // Load a test file
        string json = await File.ReadAllTextAsync("../../../capstmt.json");

        // Add data as a JSON path
        AddTransform addTrans = new()
        {
            JsonPath = "$.contact[0].telecom",
            AppendNode = "{ \"system\": \"url\", \"value\": \"https://www.microsoft2.com\" }",
        };

        // Create a collection which holds all the JSON transformatinos that need to occur
        TransformCollection transforms = new()
        {
            addTrans,
        };

        // Transform JSON
        TransformPolicy policy = new(transforms);
        string transformedJson = policy.Transform(json);

        // Output transform
        var obj = JObject.Parse(transformedJson);
        JToken jtoken = obj.SelectToken("$.contact[0].telecom");
        var jArray = jtoken as JArray;
        JToken outToken = JToken.Parse(jArray.Children<JToken>().ToArray()[1].ToString()).Children().ToArray()[1];
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Verify added json \r\n'{outToken.Parent}'");
        Console.ResetColor();
    }
}
