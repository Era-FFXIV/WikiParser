using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WikiParser
{
    [JsonSerializable(typeof(WikiFateItem))]
    internal class WikiFateItem
    {
        public int Cost { get; set; }
        public int RankRequired { get; set; }
        public uint ZoneId { get; set; } 
        public uint QuestRequired { get; set; }
        public uint ItemId { get; set; }
        public string Expansion { get; set; }
        public bool IsAllItems { get; set; }

        public WikiFateItem(string itemName, int cost, int rankRequired, string zoneName, string questRequired, string expansion, bool isAllItems = false)
        {
            var item = WikiParser.itemSheet.FirstOrDefault(i => i.Name == itemName);
            if (item.RowId == 0)
            {
                Console.WriteLine($"Could not find {itemName} in the item sheet");
                return;
            }
            ItemId = item.RowId;
            var zone = WikiParser.territoryTypeSheet.FirstOrDefault(z => z.PlaceName.Value.Name == zoneName);
            if (zone.RowId == 0 && !isAllItems)
            {
                Console.WriteLine($"Could not find {zoneName} in the territory type sheet");
                return;
            }
            var quest = WikiParser.questSheet.FirstOrDefault(q => q.Name == questRequired);
            Cost = cost;
            RankRequired = rankRequired;
            QuestRequired = questRequired != "N/A" ? quest.RowId : 0;
            ZoneId = zone.RowId;
            Expansion = expansion;
            IsAllItems = isAllItems;
        }
    }
}
