using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HereMapFinalProject
{
    internal static class Program
    {
        public static async Task Main()
        {
            const string apiKey = "### YOUR_APIKEY_HERE ###";
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("enter 1 to check if location is in street.");
            Console.WriteLine("enter 2 to check if route is in a valid direction.");
            Console.WriteLine("enter 3 to check the speed limit.");

            var input = Console.ReadLine();
            do
            {
                if (input == "1" || input == "2" || input == "3") continue;

                Console.WriteLine("Invalid Input");
                Console.WriteLine("enter 1 to check if location in street.");
                Console.WriteLine("enter 2 to check if route in valid direction.");
                Console.WriteLine("enter 3 to check speed limit.");

                input = Console.ReadLine();
            } while (input != "1" && input != "2" && input != "3");

            switch (input)
            {
                case "1":
                    try
                    {
                        var executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        var inputFile = Path.Combine(executableLocation ?? string.Empty, "street_input.csv");
                        var stringDate = DateTime.Now.ToString("MMddHHmmssff");
                        var outputFile = inputFile.Replace("street_input.csv", $"street_output_{stringDate}.csv");

                        var currentLocation = new Location();
                        var isOffRoad = true;
                        using (var reader = new StreamReader(inputFile))
                        using (var writer = new StreamWriter(outputFile))
                        {
                            string line;
                            while ((line = await reader.ReadLineAsync()) != null)
                            {
                                string newLine;
                                // Split the line into fields
                                var fields = line.Split(',');

                                if (fields[0] == "Location")
                                {
                                    newLine = fields[0] + ",Result";
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(fields[0]))
                                    {
                                        newLine = ",Location is empty";
                                    }
                                    else
                                    {
                                        currentLocation = new Location
                                        {
                                            Latitude = Convert.ToDouble(fields[0].Split('-')[0]),
                                            Longitude = Convert.ToDouble(fields[0].Split('-')[1])
                                        };
                                        isOffRoad = await OffRoad.IsOffRoad(currentLocation, apiKey);

                                        newLine = $"{currentLocation.Latitude}-{currentLocation.Longitude},{isOffRoad}";
                                    }
                                }

                                await writer.WriteLineAsync(newLine);
                            }
                        }

                        Console.WriteLine();
                        Console.WriteLine(isOffRoad
                            ? $"Road on Credentials: ({currentLocation.Latitude}, {currentLocation.Longitude}) is unpaved"
                            : $"Road on Credentials: ({currentLocation.Latitude}, {currentLocation.Longitude}) is Paved");

                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Thread.Sleep(1000);
                    }

                    break;

                case "2":
                    var result = "It's not valid";
                    try
                    {
                        //Console.WriteLine("Please enter file path ex (\"C:\\Users\\User\\Desktop\\valid_direction_input.csv\").");
                        var executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        var inputFile = Path.Combine(executableLocation ?? string.Empty, "valid_direction_input.csv");
                        var stringDate = DateTime.Now.ToString("MMddHHmmssff");
                        var outputFile = inputFile.Replace("valid_direction_input.csv",
                            $"valid_direction_output_{stringDate}.csv");

                        using (var reader = new StreamReader(inputFile))
                        using (var writer = new StreamWriter(outputFile))
                        {
                            string line;
                            while ((line = await reader.ReadLineAsync()) != null)
                            {
                                string newLine;
                                var fields = line.Split(',');

                                if (fields[0] == "Origin")
                                {
                                    newLine = fields[0] + "," + fields[1] + ",Result";
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(fields[0]) && string.IsNullOrEmpty(fields[1]))
                                    {
                                        newLine = ",,Origin and Destination are empty";
                                    }

                                    else if (string.IsNullOrEmpty(fields[0]))
                                    {
                                        newLine = $",{fields[1]},Origin is empty";
                                    }
                                    else if (string.IsNullOrEmpty(fields[1]))
                                    {
                                        newLine = $"{fields[0]},,Destination is empty";
                                    }
                                    else
                                    {
                                        var currentLocation = new Location
                                        {
                                            Latitude = Convert.ToDouble(fields[0].Split('-')[0]),
                                            Longitude = Convert.ToDouble(fields[0].Split('-')[1])
                                        };
                                        var previousLocation = new Location
                                        {
                                            Latitude = Convert.ToDouble(fields[1].Split('-')[0]),
                                            Longitude = Convert.ToDouble(fields[1].Split('-')[1])
                                        };
                                        result = await Routing.IsValidWay(previousLocation, currentLocation, apiKey);

                                        newLine = $"{previousLocation.Latitude}-{previousLocation.Longitude},{currentLocation.Latitude}-{currentLocation.Longitude},{result}";
                                    }
                                }

                                await writer.WriteLineAsync(newLine);
                            }
                        }

                        Console.WriteLine("Done");
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Thread.Sleep(1000);
                    }

                    Console.WriteLine(result);
                    break;
                case "3":
                    try
                    {
                        var executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        var inputFile = Path.Combine(executableLocation ?? string.Empty, "speed_limit_input.csv");
                        var stringDate = DateTime.Now.ToString("MMddHHmmssff");
                        var outputFile = inputFile.Replace("speed_limit_input.csv", $"speed_limit_output_{stringDate}.csv");

                        using (var reader = new StreamReader(inputFile))
                        using (var writer = new StreamWriter(outputFile))
                        {
                            string line;
                            while ((line = await reader.ReadLineAsync()) != null)
                            {
                                string newLine;
                                // Split the line into fields
                                var fields = line.Split(',');

                                if (fields[0] == "Location")
                                {
                                    newLine = fields[0] + ",Speed Limit";
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(fields[0]))
                                    {
                                        newLine = ",Location is empty";
                                    }
                                    else
                                    {
                                        var currentLocation = new Location
                                        {
                                            Latitude = Convert.ToDouble(fields[0].Split('-')[0]),
                                            Longitude = Convert.ToDouble(fields[0].Split('-')[1])
                                        };
                                        var getSpeedLimitResult =
                                            await RouteMatching.GetSpeedLimit(currentLocation, apiKey);

                                        newLine = $"{currentLocation.Latitude}-{currentLocation.Longitude},{getSpeedLimitResult}";
                                    }
                                }

                                await writer.WriteLineAsync(newLine);
                            }
                        }

                        Console.WriteLine("Done");
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Thread.Sleep(1000);
                    }

                    break;
                default:
                    Console.WriteLine("YOU HAVE ENTERED INVALID INPUT PLEASE TRY AGAIN.");
                    Thread.Sleep(1000);
                    break;
            }
        }
    }

    public static class RouteMatching
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private const string RouteMatchingEndpoint = "https://routematching.hereapi.com/v8/match/routelinks";

        public static async Task<string> GetSpeedLimit(Location location, string apiKey)
        {
            try
            {
                var queryString = $"apikey={apiKey}&routeMatch=1&mode=fastest;car;traffic:disabled;&attributes=SPEED_LIMITS_FCn(*)";
                var myContent = $"LATITUDE,LONGITUDE\r\n{location.Latitude},{location.Longitude}";
                var buffer = Encoding.UTF8.GetBytes(myContent);
                var byteContent = new ByteArrayContent(buffer);
                var response = await HttpClient.PostAsync($"{RouteMatchingEndpoint}?{queryString}", byteContent);

                if (!response.IsSuccessStatusCode) return "Failed: Request Failed";

                var json = await response.Content.ReadAsStringAsync();
                var parsedResponse = JObject.Parse(json);
                var responseObject = (JObject)parsedResponse["response"];
                var routes = (JArray)responseObject?["route"];
                var route = (JObject)routes?[0];
                var leg = (JArray)route?["leg"];
                var link = (JArray)leg?[0]["link"];
                var speedLimitsFcn = (JArray)link?[0]["attributes"]?["SPEED_LIMITS_FCN"];
                var fromRefSpeedLimit = speedLimitsFcn?[0]["FROM_REF_SPEED_LIMIT"]?.Value<int>();
                var toRefSpeedLimit = speedLimitsFcn?[0]["TO_REF_SPEED_LIMIT"]?.Value<int>();
                return Math.Max(fromRefSpeedLimit ?? 0, toRefSpeedLimit ?? 0).ToString();

            }
            catch
            {
                return "Failed: Somthing went wrong";
            }
        }
    }

    public static class Routing
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private const string RoutingEndpoint = "https://routematching.hereapi.com/v8/match/routelinks";

        public static async Task<string> IsValidWay(Location originLocation, Location destinationLocation, string apiKey)
        {
            try
            {
                var queryString = $"apiKey={apiKey}" +
                                  $"&waypoint1={destinationLocation.Latitude},{destinationLocation.Longitude}" +
                                  $"&waypoint0={originLocation.Latitude},{originLocation.Longitude}&mode=car";

                var response = await HttpClient.GetAsync($"{RoutingEndpoint}?{queryString}");

                var queryString2 = $"apiKey={apiKey}" +
                                   $"&waypoint1={destinationLocation.Latitude},{destinationLocation.Longitude}" +
                                   $"&waypoint0={originLocation.Latitude},{originLocation.Longitude}&mode=car&oneway=penalty:0.000001";

                var response2 = await HttpClient.GetAsync($"{RoutingEndpoint}?{queryString2}");

                int distanceWithoutDirection;

                if (response2.IsSuccessStatusCode)
                {
                    var json = await response2.Content.ReadAsStringAsync();
                    var parsedResponse2 = JObject.Parse(json);
                    var responseObject2 = (JObject)parsedResponse2["response"];
                    var routes2 = (JArray)responseObject2?["route"];
                    var route2 = (JObject)routes2?[0];
                    var summary2 = (JObject)route2?["summary"];
                    distanceWithoutDirection = (summary2?["distance"] ?? 0).Value<int>();
                }
                else
                {
                    return "Failed: Request Failed";
                }

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var parsedResponse = JObject.Parse(json);
                    var responseObject = (JObject)parsedResponse["response"];
                    var routes = (JArray)responseObject?["route"];
                    var route = (JObject)routes?[0];
                    var summary = (JObject)route?["summary"];
                    var distance = summary?["distance"]?.Value<int>();

                    return distance <= distanceWithoutDirection ? "Valid" : "Not Valid";
                }

                return "Failed: Request Failed";
            }
            catch
            {
                return "Failed: Somthing went wrong";
            }
        }
    }

    public static class OffRoad
    {
        public static async Task<bool> IsOffRoad(Location location, string apiKey)
        {
            var roadRefSeg = GetRoadRefSegment(location.Latitude, location.Longitude, apiKey);

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5);
                var url = $"https://smap.hereapi.com/v8/maps/attributes/segments?apikey={apiKey}";

                var body = new MultipartFormDataContent();
                body.Add(new StringContent("LINK_ATTRIBUTE_FCn(*)"), "attributes");
                body.Add(new StringContent($"$1:{roadRefSeg}"), "segmentRefs");

                var response = await client.PostAsync(url, body);
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var content = response.Content.ReadAsStringAsync().Result;
                var jsonResult = JObject.Parse(content);
                var pavedValue = jsonResult["segments"]?[0]?["attributes"]?["LINK_ATTRIBUTE_FCN"]?[0]?["PAVED"]
                    ?.ToString();

                return pavedValue == "N";
            }
        }

        private static string GetRoadRefSegment(double lat, double lng, string apiKey)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5);

                var url = $"https://revgeocode.search.hereapi.com/v1/revgeocode?at={lat},{lng}&types-street&show=hmcReference&limit=1&lang=en-US&apikey={apiKey}";

                var response = client.GetAsync(url).Result;
                if (!response.IsSuccessStatusCode)
                {
                    return string.Empty;
                }

                var content = response.Content.ReadAsStringAsync().Result;
                var jsonResult = JObject.Parse(content);

                return jsonResult["items"]?[0]?["hmc"]?["ref"]?
                    .ToString()
                    .Replace("here:cm:segment:", "");
            }
        }
    }

    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}