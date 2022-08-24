// See https://aka.ms/new-console-template for more information
using Azure.Health.DataServices.Json.Transforms;
using Newtonsoft.Json.Linq;

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
JArray jArray = jtoken as JArray;
JToken outToken = JToken.Parse(jArray.Children<JToken>().ToArray()[1].ToString()).Children().ToArray()[1];
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Verify added json \r\n'{outToken.Parent}'");
Console.ResetColor();
