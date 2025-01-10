using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace WikiParser
{
    internal class WikiParser
    {
        static void Main(string[] args)
        {
            try
            {
                RunParser().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Job's done!");
        }
        private static List<WikiFateItem> Items = [];
        private static async Task RunParser()
        {
            var targetUrl = "https://ffxiv.consolegameswiki.com/wiki/Shared_FATE";
            using HttpClient client = new(new HttpClientHandler { });
            client.DefaultRequestHeaders.Add("Encoding", "UTF-8");
            var html = await client.GetStringAsync(targetUrl);
            html = html.Replace(@"\u0027", "'").Replace("\n","");
            HtmlDocument doc = new();
            doc.LoadHtml(html);
            var container = doc.DocumentNode.SelectSingleNode("//div[@class='mw-parser-output']");
            // the table set has an h2 header called "Exchanged Items", then an h3 header with the expansion name, then a table
            var tables = container.SelectNodes("//table");
            var validIds = new List<string>(["Shadowbringers_3", "Endwalker_3", "Dawntrail_3"]);
            foreach (var table in tables)
            {
                if (table.PreviousSibling == null) break;
                if (!validIds.Contains(table.PreviousSibling.FirstChild.Id)) continue;
                var expansionName = table.PreviousSibling.InnerText;
                var rows = table.SelectNodes("tbody/tr");
                string zoneName = "";
                int index = 0;
                List<WikiFateItem> allZoneItems = [];
                foreach (var row in rows)
                {
                    var cells = row.SelectNodes("td");
                    if (cells == null) continue;
                    if (cells.Count == 1)
                    {
                        if (zoneName != "" && zoneName != "All Zones")
                        {
                            // changed zone, add all zone items to last zone
                            foreach (var item in allZoneItems)
                            {
                                var i = item;
                                i.ZoneName = zoneName;
                                Items.Add(item);
                                Console.WriteLine($"Added {item} from {zoneName} in {expansionName}");
                            }
                            allZoneItems.Clear();
                        }
                        // header row with the zone name
                        zoneName = ConvertUnicodeEscapeSequence(cells[0].InnerText);
                        continue;
                    }
                    else if (cells.Count == 0) continue;
                    // name in first sell within a link tag
                    var name = ConvertUnicodeEscapeSequence(cells[0].SelectSingleNode("a").InnerText);
                    var cost = int.Parse(cells[1].InnerText.Replace("&#160;",""));
                    if (!int.TryParse(cells[2].InnerText.Replace("+ MSQ", "").Replace("+ Quest", ""), out int r)) continue;
                    var rankRequired = r;
                    string quest = "N/A";
                    if (cells[2].SelectSingleNode("a") != null)
                    {
                        quest = ConvertUnicodeEscapeSequence(cells[2].SelectSingleNode("a").GetAttributeValue("title", ""));
                    }
                    if (zoneName == "All Zones" && name == "materia")
                    {
                        var materiaNames = Materia.GetMateriaNames(expansionName, index);
                        foreach (var materiaName in materiaNames)
                        {
                            allZoneItems.Add(new WikiFateItem(materiaName, cost, rankRequired, zoneName, quest));
                            Console.WriteLine($"Added {materiaName} from {zoneName} in {expansionName} which needs quest {quest}");
                        }
                        index++;
                        continue;
                    }
                    else if (zoneName == "All Zones")
                    {
                        allZoneItems.Add(new WikiFateItem(name, cost, rankRequired, zoneName, quest));
                        continue;
                    }
                    Items.Add(new WikiFateItem(name, cost, rankRequired, zoneName, quest));
                    Console.WriteLine($"Added {name} from {zoneName} in {expansionName} which needs quest {quest}");
                }
            }
            // write the items to a json file
            var json = JsonSerializer.Serialize(Items);
            await File.WriteAllTextAsync("items.json", json);
        }

        static string ConvertUnicodeEscapeSequence(string input)
        {
            // Replace the Unicode escape sequences (\uXXXX) with their corresponding characters
            return input.Replace(@"\u0027", "'");
        }
        static class Materia
        {
            private static Dictionary<string, List<string>> NumsByExpansion = new()
            {
                { "Shadowbringers", ["VII", "VIII"] },
                { "Endwalker", ["IX", "X"] },
                { "Dawntrail", ["XI", "XII"] }
            };
            private static List<string> MateriaTypes = new()
            {
                "Heavens' Eye Materia",
                "Quickarm Materia",
                "Savage Aim Materia",
                "Savage Might Materia",
                "Battledance Materia",
                "Piety Materia",
                "Quicktongue Materia"
            };
            public static List<string> GetMateriaNames(string expansion, int index)
            {
                var num = NumsByExpansion[expansion][index];
                var names = new List<string>();
                foreach (var type in MateriaTypes)
                {
                    names.Add($"{type} {num}");
                }
                return names;
            }
        }
    }
}
