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
            var baron =
                    MinionManager.GetMinions(
                        ObjectManager.Player.ServerPosition,
                        SpellManager.Spell[SpellSlot.E].Range,
                        MinionTypes.All,
                        MinionTeam.Neutral,
                        MinionOrderTypes.MaxHealth)
                        .FirstOrDefault(
                            x => x.IsValidTarget() && HealthPrediction.GetHealthPrediction(x, 250) + 5 < this.GetBaronReduction(x) && x.Name.Contains("Baron"));

            var dragon =
                MinionManager.GetMinions(
                    ObjectManager.Player.ServerPosition,
                    SpellManager.Spell[SpellSlot.E].Range,
                    MinionTypes.All,
                    MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth)
                    .FirstOrDefault(
                        x => x.IsValidTarget() && HealthPrediction.GetHealthPrediction(x, 250) + 5 < this.GetDragonReduction(x) && x.Name.Contains("Dragon"));

            if ((dragon != null && SpellManager.Spell[SpellSlot.E].CanCast(dragon)) || (baron != null && SpellManager.Spell[SpellSlot.E].CanCast(baron)))
            {
                SpellManager.Spell[SpellSlot.E].Cast();
            }
        }

        /// <summary>
        /// Gets the baron reduction.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        private float GetBaronReduction(Obj_AI_Base target)
        {
            return ObjectManager.Player.HasBuff("barontarget")
                       ? SpellManager.Spell[SpellSlot.E].GetDamage(target) * 0.5f
                       : SpellManager.Spell[SpellSlot.E].GetDamage(target);
        }


        /// <summary>
        /// Gets the dragon reduction.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        private float GetDragonReduction(Obj_AI_Base target)
        {
            return ObjectManager.Player.HasBuff("s5test_dragonslayerbuff")
                       ? SpellManager.Spell[SpellSlot.E].GetDamage(target)
                         * (1 - (.07f * ObjectManager.Player.GetBuffCount("s5test_dragonslayerbuff")))
                       : SpellManager.Spell[SpellSlot.E].GetDamage(target);
        }

    }
}