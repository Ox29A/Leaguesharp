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
                MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition,
                    SpellManager.Spell[SpellSlot.E].Range,
                    MinionTypes.All,
                    MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(x => x.IsRendKillable() && !x.Name.Contains("Mini") && x.Name.Contains("SRU_"));

            if (attackableMinion != null && SpellManager.Spell[SpellSlot.E].CanCast(attackableMinion))
            {
                foreach (var minion in Kalista.JungleMinions)
                {
                    if (minion.Key.Contains(attackableMinion.CharData.BaseSkinName) &&
                        Kalista.Menu.Item(attackableMinion.CharData.BaseSkinName).GetValue<bool>())
                    {
                        Console.WriteLine("Minion: " + attackableMinion.CharData.BaseSkinName);
                        Console.WriteLine("Key: " + minion.Value);
                        SpellManager.Spell[SpellSlot.E].Cast();
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the baron reduction.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        private float GetBaronReduction(Obj_AI_Base target)
        {
            return ObjectManager.Player.HasBuff("barontarget")
                ? SpellManager.Spell[SpellSlot.E].GetDamage(target)*0.5f
                : SpellManager.Spell[SpellSlot.E].GetDamage(target);
        }


        /// <summary>
        ///     Gets the dragon reduction.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        private float GetDragonReduction(Obj_AI_Base target)
        {
            return ObjectManager.Player.HasBuff("s5test_dragonslayerbuff")
                ? SpellManager.Spell[SpellSlot.E].GetDamage(target)
                  *(1 - (.07f*ObjectManager.Player.GetBuffCount("s5test_dragonslayerbuff")))
                : SpellManager.Spell[SpellSlot.E].GetDamage(target);
        }
    }
}