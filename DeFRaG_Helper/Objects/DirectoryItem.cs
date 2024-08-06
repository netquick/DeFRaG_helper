using System.IO;

namespace DeFRaG_Helper.Objects
{
    public class DirectoryItem
    {
        public string Name { get; }
        public DirectoryInfo DirectoryInfo { get; }

        public DirectoryItem(DirectoryInfo directoryInfo)
        {
            DirectoryInfo = directoryInfo;
            Name = directoryInfo.Name;
        }
    }
}
