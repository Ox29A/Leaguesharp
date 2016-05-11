using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZLib.MenuExtensions;
using LeagueSharp.Common;

namespace iLucian.MenuHelper
{
    class MenuGenerator
    {
        public static void Generate()
        {
            Variables.Menu = new Menu("iLucian", "com.ilucian", true);
            var rootMenu = Variables.Menu;

            var owMenu = new Menu(":: iLucian - Orbwalker", "com.ilucian.orbwalker");
            {
                Variables.Orbwalker = new Orbwalking.Orbwalker(owMenu);
                rootMenu.AddSubMenu(owMenu);
            }

            var comboOptions = new Menu(":: iLucian - Combo Options", "com.ilucian.combo");
            {
                comboOptions.AddBool("com.ilucian.combo.q", "Use Q", true);
                comboOptions.AddBool("com.ilucian.combo.qExtended", "Use Extended Q", true);
                comboOptions.AddBool("com.ilucian.combo.w", "Use W", true);
                comboOptions.AddBool("com.ilucian.combo.e", "Use E", true);
                comboOptions.AddStringList("com.ilucian.combo.eMode", "E Mode",
                    new[] {"Kite", "Side", "Cursor", "Enemy"});
                rootMenu.AddSubMenu(comboOptions);
            }


            var harassOptions = new Menu(":: iLucian - Harass Options", "com.ilucian.harass");
            {
                harassOptions.AddBool("com.ilucian.harass.q", "Use Q", true);
                harassOptions.AddBool("com.ilucian.harass.qExtended", "Use Extended Q", true);
                harassOptions.AddBool("com.ilucian.harass.w", "Use W", true);
                rootMenu.AddSubMenu(harassOptions);
            }

            var laneclearOptions = new Menu(":: iLucian - Laneclear Options", "com.ilucian.laneclear");
            {
                laneclearOptions.AddBool("com.ilucian.laneclear.q", "Use Q", true);
                laneclearOptions.AddSlider("com.ilucian.laneclear.qMinions", "Cast Q on x minions", 3, 1, 10);
                rootMenu.AddSubMenu(laneclearOptions);
            }

            var miscOptions = new Menu(":: iLucian - Misc Options", "com.ilucian.misc");
            {
                miscOptions.AddBool("com.ilucian.misc.usePrediction", "Use W Pred", true);
                miscOptions.AddBool("com.ilucian.misc.gapcloser", "Use E For Gapcloser", true);
                miscOptions.AddBool("com.ilucian.misc.eqKs", "EQ - Killsteal", true);
                miscOptions.AddBool("com.ilucian.misc.useChampions", "Use EQ on Champions", true);
                rootMenu.AddSubMenu(miscOptions);
            }

            rootMenu.AddToMainMenu();
        }
    }
}