// See https://aka.ms/new-console-template for more information
using DataServices.Json.Transforms;
using Newtonsoft.Json.Linq;

string json = await File.ReadAllTextAsync("../../../capstmt.json");
AddTransform addTrans = new()
{
    JsonPath = "$.contact[0].telecom",
    AppendNode = "{ \"system\": \"url\", \"value\": \"https://www.microsoft2.com\" }",
};

TransformCollection transforms = new()
{
    addTrans,
};



TransformPolicy policy = new(transforms);
string transformedJson = policy.Transform(json);

var obj = JObject.Parse(transformedJson);
JToken jtoken = obj.SelectToken("$.contact[0].telecom");
JArray jArray = jtoken as JArray;
JToken outToken = JToken.Parse(jArray.Children<JToken>().ToArray()[1].ToString()).Children().ToArray()[1];
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Verify added json \r\n'{outToken.Parent}'");
Console.ResetColor();
