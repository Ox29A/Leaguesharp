using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZLib.MenuExtensions;
using LeagueSharp.Common;
using Color = SharpDX.Color;

namespace iLucian.MenuHelper
{
    class MenuGenerator
    {
        public static void Generate()
        {
            Variables.Menu = new Menu("iLucian", "com.ilucian", true).SetFontStyle(FontStyle.Bold, Color.DeepPink);
            var rootMenu = Variables.Menu;

            var owMenu = new Menu(":: iLucian - Orbwalker", "com.ilucian.orbwalker");
            {
                Variables.Orbwalker = new Orbwalking.Orbwalker(owMenu);
                rootMenu.AddSubMenu(owMenu);
            }

            var comboOptions = new Menu(":: iLucian - Combo Options", "com.ilucian.combo").SetFontStyle(FontStyle.Regular, Color.Aqua);
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
                var autoHarassMenu = new Menu("Auto Harass", "com.ilucian.harass.auto");
                {
                    autoHarassMenu.AddKeybind("com.ilucian.harass.auto.autoharass", "Enabled",
                        new Tuple<uint, KeyBindType>("T".ToCharArray()[0], KeyBindType.Toggle)).Permashow(true, "iLucian | Auto Harass", Color.Aqua);
                    autoHarassMenu.AddBool("com.ilucian.harass.auto.q", "Use Q", true);
                    autoHarassMenu.AddBool("com.ilucian.harass.auto.qExtended", "Use Extended Q", true);
                }
                harassOptions.AddBool("com.ilucian.harass.q", "Use Q", true);
                harassOptions.AddBool("com.ilucian.harass.qExtended", "Use Extended Q", true);
                harassOptions.AddBool("com.ilucian.harass.w", "Use W", true);
                harassOptions.AddSubMenu(autoHarassMenu);
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
                miscOptions.AddBool("com.ilucian.misc.extendChamps", "Use Ext Q on Champions", true);
                rootMenu.AddSubMenu(miscOptions);
            }

            rootMenu.AddToMainMenu();
        }
    }
}