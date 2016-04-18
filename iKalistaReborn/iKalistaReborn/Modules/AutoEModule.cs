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
    internal class AutoEModule : IModule
    {
        public void OnLoad()
        {
            Console.WriteLine("Auto E Module Loaded");
        }

        public string GetName()
        {
            return "AutoEHarass";
        }

        public bool ShouldGetExecuted()
        {
            return SpellManager.Spell[SpellSlot.E].IsReady() &&
                   Kalista.Menu.Item("com.ikalista.combo.autoE").GetValue<bool>() &&
                   Kalista.Menu.Item("com.ikalista.modules." + GetName().ToLowerInvariant()).GetValue<bool>();
        }

        public ModuleType GetModuleType()
        {
            return ModuleType.OnUpdate;
        }

        public void OnExecute()
        {
            var enemy = HeroManager.Enemies.Where(hero => hero.HasRendBuff()).MinOrDefault(hero => hero.Distance(ObjectManager.Player, true));
            if (enemy?.Distance(ObjectManager.Player, true) < Math.Pow(SpellManager.Spell[SpellSlot.E].Range + 200, 2))
            {
                if (ObjectManager.Get<Obj_AI_Minion>().Any(x => SpellManager.Spell[SpellSlot.E].IsInRange(x) && x.HasRendBuff() && Helper.GetRendDamage(x) >= x.Health))
                {
                    SpellManager.Spell[SpellSlot.E].Cast();
                }
            }
        }
    }
}