using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DeFRaG_Helper.Helpers
{
    public static class MapDetailProcessor
    {
        public static void ProcessProperty(string propertyName, string propertyValue, Map map)
        {
            switch (propertyName)
            {
                case "Mapname":
                    map.Name = propertyValue;
                    break;
                case "Filename":
                    // Use Regex to remove <span> tag and its content from the filename
                    var cleanFilenameRegex = new Regex(@"(.+?)<span.*?>.*?</span>", RegexOptions.Singleline);
                    var cleanMatch = cleanFilenameRegex.Match(propertyValue);
                    if (cleanMatch.Success)
                    {
                        // If the pattern matches, use the cleaned filename and append the .bsp extension
                        map.Mapname = DecodeHtmlString(cleanMatch.Groups[1].Value + ".bsp");
                    }
                    else
                    {
                        // If there's no <span> tag, use the original propertyValue and append the .bsp extension
                        map.Mapname = propertyValue + ".bsp";
                    }
                    SimpleLogger.Log("Mapname: " + map.Mapname);
                    break;

                case "Pk3 file":
                    // Use Regex to extract the filename from the <a> href attribute, including the .pk3 extension
                    var pk3FilenameRegex = new Regex(@"<a href=""/maps/downloads/(.+?\.pk3)""", RegexOptions.Singleline);
                    var pk3FilenameMatch = pk3FilenameRegex.Match(propertyValue);
                    if (pk3FilenameMatch.Success)
                    {
                        // If the pattern matches, use the captured filename including the .pk3 extension
                        map.Filename = pk3FilenameMatch.Groups[1].Value;
                    }
                    else
                    {
                        // If there's no match, you might want to handle this case, e.g., log an error or assign a default value
                        map.Filename = "Unknown Filename";
                    }
                    break;




                case "Author":
                    // Use Regex to extract the author's name from the <a> tag
                    var authorRegex = new Regex(@"<a href=""/maps/\?map=&amp;au=(.+?)"">(.+?)</a>", RegexOptions.Singleline);
                    var authorMatch = authorRegex.Match(propertyValue);
                    if (authorMatch.Success)
                    {
                        // If the pattern matches, use the author's name
                        map.Author = DecodeHtmlString(authorMatch.Groups[2].Value);
                    }
                    else
                    {
                        // If there's no match, you might want to handle this case, e.g., log an error or assign a default value
                        map.Author = "Unknown Author";
                    }
                    break;

                case "Modification":
                    var imgTitleRegex = new Regex(@"<img[^>]+title=""([^""]+)""");
                    var imgTitleMatch = imgTitleRegex.Match(propertyValue);
                    if (imgTitleMatch.Success)
                    {
                        map.GameType = imgTitleMatch.Groups[1].Value;
                    }
                    else
                    {
                        map.GameType = "Unknown";
                    }
                    break;
                case "Defrag style":
                    // Use Regex to extract the Defrag style from the <a> tag
                    var styleRegex = new Regex(@"<a rel=""help"" href=""/legend/#lg-dftyle"" title=""(.+?)"">(.+?)</a>", RegexOptions.Singleline);
                    var styleMatch = styleRegex.Match(propertyValue);
                    if (styleMatch.Success)
                    {
                        // If the pattern matches, use the Defrag style text
                        map.Style = styleMatch.Groups[2].Value;
                    }
                    else
                    {
                        // If there's no match, you might want to handle this case, e.g., log an error or assign a default value
                        map.Style = "Unknown Style";
                    }
                    break;



                case "Defrag physics":
                    // Initialize a variable to hold the physics value
                    int physicsValue = 0;

                    // Check for the presence of "vq3" and "cpm" using Regex
                    bool hasVQ3 = Regex.IsMatch(propertyValue, @"Vanilla Quake 3", RegexOptions.IgnoreCase);
                    bool hasCPM = Regex.IsMatch(propertyValue, @"Challenge Promode", RegexOptions.IgnoreCase);

                    if (hasVQ3 && hasCPM)
                    {
                        // Both VQ3 and CPM are present
                        physicsValue = 3;
                    }
                    else if (hasVQ3)
                    {
                        // Only VQ3 is present
                        physicsValue = 1;
                    }
                    else if (hasCPM)
                    {
                        // Only CPM is present
                        physicsValue = 2;
                    }
                    else
                    {
                        // Neither VQ3 nor CPM is present, handle as needed
                        physicsValue = 0; // You might want to log this case or assign a default value
                    }

                    // Assign the determined physics value to the map object
                    map.Physics = physicsValue;
                    break;



                case "Release date":
                    // Use Regex to extract the date from the datetime attribute of the <time> tag
                    var dateRegex = new Regex(@"<time datetime=""(.+?)"">", RegexOptions.Singleline);
                    var dateMatch = dateRegex.Match(propertyValue);
                    if (dateMatch.Success)
                    {
                        // If the pattern matches, use the captured date
                        string dateString = dateMatch.Groups[1].Value;



                        // If your map object expects a string for the release date, you can directly assign the dateString
                        map.Releasedate = dateString;
                    }
                    else
                    {
                        // If there's no match, you might want to handle this case, e.g., log an error or assign a default value
                        map.Releasedate = "Unknown Date";
                    }
                    break;

                case "File size":
                    // Use Regex to extract the numeric part of the file size and its unit (MB or KB)
                    var fileSizeRegex = new Regex(@"(\d+(\.\d+)?)\s*(MB|KB)", RegexOptions.IgnoreCase);
                    var fileSizeMatch = fileSizeRegex.Match(propertyValue);
                    if (fileSizeMatch.Success)
                    {
                        // Extracted numeric part of the file size
                        double fileSizeValue = double.Parse(fileSizeMatch.Groups[1].Value);
                        // Extracted unit (MB or KB)
                        string fileSizeUnit = fileSizeMatch.Groups[3].Value;

                        // Convert file size to bytes or handle as needed
                        long fileSizeInBytes = 0;
                        if (fileSizeUnit.Equals("MB", StringComparison.OrdinalIgnoreCase))
                        {
                            fileSizeInBytes = (long)(fileSizeValue * 1024 * 1024);
                        }
                        else if (fileSizeUnit.Equals("KB", StringComparison.OrdinalIgnoreCase))
                        {
                            fileSizeInBytes = (long)(fileSizeValue * 1024);
                        }

                        // Assign the converted file size to the map object
                        // Assuming your map object has a property for file size in bytes
                        map.Size = fileSizeInBytes;
                    }
                    else
                    {
                        // If there's no match, you might want to handle this case, e.g., log an error or assign a default value
                        map.Size = 0; // Example default value
                    }
                    break;

                case "Downloads":
                    if (int.TryParse(propertyValue, out int downloads))
                    {
                        map.Hits = downloads;
                    }
                    else
                    {
                        map.Hits = 0;
                    }
                    break;
                case "Defrag online records":
                    var vq3Regex = new Regex(@"<a href=""(https?://www\.q3df\.org/records/details\?physic=0&map=[^""]+)"">vq3</a>");
                    var cpmRegex = new Regex(@"<a href=""(https?://www\.q3df\.org/records/details\?physic=1&map=[^""]+)"">cpm</a>");


                    var vq3Match = vq3Regex.Match(propertyValue);
                    if (vq3Match.Success)
                    {
                        map.LinksOnlineRecordsQ3DFVQ3 = vq3Match.Groups[1].Value;
                    }

                    var cpmMatch = cpmRegex.Match(propertyValue);
                    if (cpmMatch.Success)
                    {
                        map.LinksOnlineRecordsQ3DFCPM = cpmMatch.Groups[1].Value;
                    }
                    break;
                case "":
                    var racingVq3Regex = new Regex(@"<a href=""(https?://defrag\.racing/maps/[^?]+\?gametype=run_vq3)"">vq3</a>");
                    var racingCpmRegex = new Regex(@"<a href=""(https?://defrag\.racing/maps/[^?]+\?gametype=run_cpm)"">cpm</a>");

                    var racingVq3Match = racingVq3Regex.Match(propertyValue);
                    if (racingVq3Match.Success)
                    {
                        map.LinksOnlineRecordsRacingVQ3 = "https:" + racingVq3Match.Groups[1].Value;
                    }

                    var racingCpmMatch = racingCpmRegex.Match(propertyValue);
                    if (racingCpmMatch.Success)
                    {
                        map.LinksOnlineRecordsRacingCPM = "https:" + racingCpmMatch.Groups[1].Value;
                    }
                    break;
                case "Defrag demos":
                    var demosVq3Regex = new Regex(@"<a href=""(http://defrag\.ru/defrag/maps_show\.php\?name=[^""]+)"">vq3</a>");
                    var demosCpmRegex = new Regex(@"<a href=""(http://defrag\.ru/defrag/cpm/maps_show\.php\?name=[^""]+)"">cpm</a>");


                    var demosVq3Match = demosVq3Regex.Match(propertyValue);
                    if (demosVq3Match.Success)
                    {
                        map.LinkDemosVQ3 = demosVq3Match.Groups[1].Value;
                    }

                    var demosCpmMatch = demosCpmRegex.Match(propertyValue);
                    if (demosCpmMatch.Success)
                    {
                        map.LinkDemosCPM = demosCpmMatch.Groups[1].Value;
                    }
                    break;
                case "Map dependencies":
                    // Use Regex to extract the text following "Textures:" up to the next HTML tag or end of string
                    var dependenciesRegex = new Regex(@"Textures:</span>\s*({.*?}|[^<]+)", RegexOptions.IgnoreCase);
                    var dependenciesMatch = dependenciesRegex.Match(propertyValue);
                    if (dependenciesMatch.Success)
                    {
                        // Extracted dependencies text
                        string dependenciesText = dependenciesMatch.Groups[1].Value.Trim();

                        // Assign the extracted text to the map object
                        // Assuming your map object has a property for storing dependencies text
                        map.Dependencies = dependenciesText;
                    }
                    else
                    {
                        // If there's no match, you might want to handle this case, e.g., log an error or assign a default value
                        map.Dependencies = "Unknown Dependencies";
                    }
                    break;

                case "Weapons":
                case "Items":
                case "Functions":
                    var regex = new Regex(@"<img[^>]+title=""([^""]+)""", RegexOptions.IgnoreCase);
                    var matches = regex.Matches(propertyValue);
                    foreach (Match match in matches)
                    {
                        if (match.Success)
                        {
                            string name = match.Groups[1].Value;
                            switch (propertyName)
                            {
                                case "Weapons":
                                    map.Weapons.Add(name);
                                    break;
                                case "Items":
                                    map.Items.Add(name);
                                    break;
                                case "Functions":
                                    map.Functions.Add(name);
                                    break;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }


        public static string DecodeHtmlString(string htmlString)
        {
            return System.Net.WebUtility.HtmlDecode(htmlString);
        }
    }
}
