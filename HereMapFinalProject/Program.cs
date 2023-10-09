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
                                        isOffRoad = await OffRoad.IsOffRoad(currentLocation);

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

                        Thread.Sleep(1500);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR.");
                        Console.WriteLine(ex.Message);
                        Thread.Sleep(1500);
                    }

                    break;

                case "2":
                    var result = "It's not valid";
                    try
                    {
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
                                        result = await RouteMatching.ForbiddenRoadDirection(previousLocation, currentLocation) ? "Forbidden Road Direction" : "Valid Road Direction";

                                        newLine = $"{previousLocation.Latitude}-{previousLocation.Longitude},{currentLocation.Latitude}-{currentLocation.Longitude},  {result}";
                                    }
                                }

                                await writer.WriteLineAsync(newLine);
                            }
                        }

                        Console.WriteLine(result);
                        Console.WriteLine("Done");
                        Console.ReadLine();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR.");
                        Console.WriteLine(ex.Message);
                        Thread.Sleep(1500);
                    }

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
                                        var getSpeedLimitResult = await RouteMatching.GetSpeedLimit(currentLocation);

                                        newLine = $"{currentLocation.Latitude}-{currentLocation.Longitude},{getSpeedLimitResult}";
                                    }
                                }

                                await writer.WriteLineAsync(newLine);
                            }
                        }

                        Console.WriteLine("Done");
                        Console.ReadLine();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR.");
                        Console.WriteLine(ex.Message);
                        Thread.Sleep(1500);
                    }

                    break;
                default:
                    Console.WriteLine("YOU HAVE ENTERED INVALID INPUT PLEASE TRY AGAIN.");
                    Thread.Sleep(1500);
                    break;
            }
        }
    }

    public static class RouteMatching
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private const string ApiKey = "### YOUR_APIKEY_HERE ###";
        private const string RouteMatchingEndpoint = "https://routematching.hereapi.com/v8/match/routelinks";

        public static async Task<string> GetSpeedLimit(Location location)
        {
            try
            {
                var queryString = $"apikey={ApiKey}&routeMatch=1&mode=fastest;car;traffic:disabled;&attributes=SPEED_LIMITS_FCn(*)";
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

        public static async Task<bool> ForbiddenRoadDirection(Location origin, Location destination)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"https://routematching.hereapi.com/v8/match/routelinks?routeMatch=1&mode=fastest;car;traffic:disabled&apiKey={ApiKey}&drivingReport=1");

                var requestBody = "LATITUDE,LONGITUDE" +
                          $"\r\n{destination.Latitude},{destination.Longitude}" +
                          $"\r\n{origin.Latitude},{origin.Longitude}";

                request.Content = new StringContent(requestBody);

                var response = await HttpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var jsonObject = JObject.Parse(result);

                    var message = (string)jsonObject["response"]?["warnings"]?[0]?["message"];
                    var illegalRoadDirection = message != null && message.EndsWith("forbidden driving direction");

                    if (illegalRoadDirection)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    public static class OffRoad
    {
        private const string ApiKey = "### YOUR_APIKEY_HERE ###";

        public static async Task<bool> IsOffRoad(Location location)
        {
            var roadRefSeg = GetRoadRefSegment(location.Latitude, location.Longitude, ApiKey);

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5);
                var url = $"https://smap.hereapi.com/v8/maps/attributes/segments?apikey={ApiKey}";

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