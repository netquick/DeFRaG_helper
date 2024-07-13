using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeFRaG_Helper
{
    public interface ISearchableItem
    {
        string DisplayName { get; }
        string NavigationPath { get; }
        string MapIdentifier { get; }

    }

}
