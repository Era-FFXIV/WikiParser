using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WikiParser
{
    [JsonSerializable(typeof(WikiFateItem))]
    internal class WikiFateItem(string name, int cost, int rankRequired, string zoneName, string questRequired = "")
    {
        public string Name { get; set; } = name;
        public int Cost { get; set; } = cost;
        public int RankRequired { get; set; } = rankRequired;
        public string ZoneName { get; set; } = zoneName;
        public string QuestRequired { get; set; } = questRequired;
    }
}
