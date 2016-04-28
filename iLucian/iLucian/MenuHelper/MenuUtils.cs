using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;

namespace iLucian.MenuHelper
{
    static class MenuUtils
    {

        public static bool IsEnabled(this Menu menu, string item)
        {
            return menu.Item(item).GetValue<bool>();
        }

    }
}
