using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZLib.Modules;
using iKalistaReborn.Utils;
using LeagueSharp;
using LeagueSharp.Common;

namespace iKalistaReborn.Modules
{
    internal class JungleStealModule : IModule
    {
        public void OnLoad()
        {
            Console.WriteLine("Jungle Steal Module Loaded");
        }

        public string GetName()
        {
            return "JungleSteal";
        }

        public bool ShouldGetExecuted()
        {
            return SpellManager.Spell[SpellSlot.E].IsReady() &&
                   Kalista.Menu.Item("com.ikalista.modules." + GetName().ToLowerInvariant()).GetValue<bool>();
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.OnUpdate;
        }

        public void OnExecute()
        {
            var junglelMinions =
                MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition,
                    SpellManager.Spell[SpellSlot.E].Range,
                    MinionTypes.All,
                    MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(
                        x =>
                            x.IsMobKillable() && !x.Name.Contains("Mini")
                            && !x.Name.Contains("Dragon") && !x.Name.Contains("Baron"));

            var baron =
                MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition,
                    SpellManager.Spell[SpellSlot.E].Range,
                    MinionTypes.All,
                    MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(x => x.IsValid && x.IsMobKillable() && x.Name.Contains("Baron"));

            var dragon =
                MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition,
                    SpellManager.Spell[SpellSlot.E].Range,
                    MinionTypes.All,
                    MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(
                        x => x.IsValid && x.IsMobKillable() && x.Name.Contains("Dragon"));

            switch (Kalista.Menu.Item("com.ikalista.jungleSteal.mode").GetValue<StringList>().SelectedIndex)
            {
                case 0: // Objectives / Baron / Dragon
                    if (baron != null && SpellManager.Spell[SpellSlot.E].CanCast(baron) ||
                        dragon != null && SpellManager.Spell[SpellSlot.E].CanCast(dragon))
                    {
                        SpellManager.Spell[SpellSlot.E].Cast();
                    }
                    break;
                case 1: // All Mobs
                    if (junglelMinions != null)
                    {
                        SpellManager.Spell[SpellSlot.E].Cast();
                    }
                    break;
                case 2: // Both options
                    if (baron != null && SpellManager.Spell[SpellSlot.E].CanCast(baron) ||
                        dragon != null && SpellManager.Spell[SpellSlot.E].CanCast(dragon))
                    {
                        SpellManager.Spell[SpellSlot.E].Cast();
                    }
                    if (junglelMinions != null)
                    {
                        SpellManager.Spell[SpellSlot.E].Cast();
                    }
                    break;
            }
        }
    }
}