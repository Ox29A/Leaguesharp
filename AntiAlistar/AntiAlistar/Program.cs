using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiAlistar
{
    using LeagueSharp.Common;

    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += AntiAlistar.OnLoad;
        }
    }
}
