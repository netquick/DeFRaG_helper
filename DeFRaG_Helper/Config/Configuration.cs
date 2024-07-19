using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeFRaG_Helper
{
    public class Configuration
    {
        public string? GameDirectoryPath { get; set; }
        public string? SelectedColor { get; set; }
        public string? ButtonState { get; set; }
        public string? PhysicsSetting { get; set; }
        public string? DatabasePath { get; set; }
        public string? DatabaseUrl { get; set; }
        public string? MenuState { get; set; }
        public string? ConnectionString { get; set; }
        public bool? UseUnsecureConnection { get; set; }
        public bool? DownloadImagesOnUpdate { get; set; }
        public int? CountHistory { get; set; }
        public bool? UseHighQualityImages { get; set; }
    }
}
