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
            var mob =
                ObjectManager.Get<Obj_AI_Minion>()
                    .FirstOrDefault(
                        x =>
                            SpellManager.Spell[SpellSlot.E].IsInRange(x) &&
                            x.IsValidTarget(SpellManager.Spell[SpellSlot.E].Range));

            if (mob == null || mob.CharData.BaseSkinName.Contains("Mini") || !mob.CharData.BaseSkinName.Contains("SRU_"))
                return;

            if (mob.IsRendKillable() && Kalista.JungleMinions.Contains(mob.CharData.BaseSkinName) && Kalista.Menu.Item("com.ikalista.jungleSteal." + mob.CharData.BaseSkinName).GetValue<bool>())
            {
                SpellManager.Spell[SpellSlot.E].Cast();
            }
        }
    }
}