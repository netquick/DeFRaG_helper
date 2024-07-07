using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DeFRaG_Helper
{
    public class Map
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? MapName { get; set; }
        public string? FileName { get; set; }
        public string? Author { get; set; }
        public string? GameType { get; set; }
        public string? ReleaseDate { get; set; }
        public string? Style { get; set; }
        public long Size { get; set; }
        public int Physics { get; set; }
        public int Hits { get; set; }
        public string? Downloadlink { get; set; }
        public int ? isDownloaded { get; set; }
        public int ? isArchived { get; set; }
        public int ? isFavorite { get; set; }

    }
}
