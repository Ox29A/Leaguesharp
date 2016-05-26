using System;
using System.Linq;
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
                   Kalista.Menu.Item("com.ikalista.jungleSteal.enabled").GetValue<bool>();
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.OnUpdate;
        }

        public void OnExecute()
        {
            var attackableMinion =
                MinionManager.GetMinions(ObjectManager.Player.ServerPosition, SpellManager.Spell[SpellSlot.E].Range,
                    MinionTypes.All, MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth).FirstOrDefault(x => !x.Name.Contains("Mini"));

            if (attackableMinion == null || !attackableMinion.HasRendBuff() || !attackableMinion.IsMobKillable() ||
                !Kalista.Menu.Item(attackableMinion.CharData.BaseSkinName).GetValue<bool>())
                return;

            Console.WriteLine("Minion Killable: " + attackableMinion.CharData.BaseSkinName);
            SpellManager.Spell[SpellSlot.E].Cast();
        }
    }
}