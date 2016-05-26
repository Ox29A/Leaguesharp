using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLucian.Utils
{
    using System.Collections;

    using iLucian.MenuHelper;

    using LeagueSharp;
    using LeagueSharp.Common;

    static class ManaMeneger
    {
        public static void AddManaManager(this Menu menu, Mode mode, SpellSlot[] spells, int[] manaCosts)
        {
            var subMenu = new Menu(":: Mana Meneger", "com.ilucian.mana");
            {
                for (var i = 0; i < spells.Count(); i++)
                {
                    subMenu.AddItem(
                        new MenuItem(
                            "com.ilucian.mana." + GetStringFromSpellSlot(spells[i]).ToLowerInvariant() + "mana"
                            + GetStringFromMode(mode).ToLowerInvariant(), 
                            "Min "+GetStringFromSpellSlot(spells[i]) + " Mana %").SetValue(new Slider(manaCosts[i])));
                }
            }
            menu.AddSubMenu(subMenu);
        }

        public static bool IsEnabledAndReady(this Spell spell, Mode mode)
        {
            if (ObjectManager.Player.IsDead)
            {
                return false;
            }
            try
            {
                var manaPercentage =
                    Variables.Menu.Item(
                        "com.ilucian.mana." + GetStringFromSpellSlot(spell.Slot).ToLowerInvariant() + "mana" +
                        GetStringFromMode(mode).ToLowerInvariant()).GetValue<Slider>().Value;
                var enabledCondition =
                    Variables.Menu.IsEnabled(
                        "com.ilucian."+GetStringFromMode(mode).ToLowerInvariant()+ "." + GetStringFromSpellSlot(spell.Slot).ToLowerInvariant());
                return spell.IsReady() && (ObjectManager.Player.ManaPercent >= manaPercentage) && enabledCondition;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return false;
        }

        public static string GetStringFromSpellSlot(SpellSlot sp)
        {
            switch (sp)
            {
                case SpellSlot.Q:
                    return "Q";
                case SpellSlot.W:
                    return "W";
                case SpellSlot.E:
                    return "E";
                case SpellSlot.R:
                    return "R";
                default:
                    return "unk";
            }
        }

        public static string GetStringFromMode(Mode mode)
        {
            switch (mode)
            {
                case Mode.Combo:
                    return "C";
                case Mode.Harass:
                    return "H";
                case Mode.Laneclear:
                    return "LC";
                default:
                    return "unk";
            }
        }

        internal enum Mode
        {
            Combo, 

            Harass, 

            Laneclear
        }
    }
}