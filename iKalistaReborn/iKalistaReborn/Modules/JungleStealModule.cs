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
            var minion =
                ObjectManager.Get<Obj_AI_Minion>()
                    .FirstOrDefault(
                        x =>
                            SpellManager.Spell[SpellSlot.E].IsInRange(x) &&
                            x.IsValidTarget(SpellManager.Spell[SpellSlot.E].Range));

            if (minion == null || minion.CharData.BaseSkinName.Contains("Mini") ||
                !minion.CharData.BaseSkinName.Contains("SRU_"))
                return;
            if (Kalista.JungleMinions.Contains(minion.CharData.BaseSkinName) &&
                Kalista.Menu.Item(minion.CharData.BaseSkinName).GetValue<bool>())
            {
                if (minion.IsMobKillable())
                {
                    SpellManager.Spell[SpellSlot.E].Cast();
                }
            }
        }
    }
}