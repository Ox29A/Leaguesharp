using System;
using System.Collections.Generic;
using System.Linq;
using DZLib.MenuExtensions;
using DZLib.Modules;
using iKalistaReborn.Modules;
using iKalistaReborn.Utils;
using LeagueSharp;
using LeagueSharp.Common;
using SPrediction;

namespace iKalistaReborn
{
    internal class Kalista
    {
        public static Menu Menu;

        /// <summary>
        ///     The Modules
        /// </summary>
        public static readonly List<IModule> Modules = new List<IModule>
        {
            new AutoRendModule(),
            new JungleStealModule()
        };

        public static Orbwalking.Orbwalker Orbwalker;

        public Kalista()
        {
            CreateMenu();
            LoadModules();
            SPrediction.Prediction.Initialize(Menu);
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnProcessSpellCast += (sender, args) =>
            {
                if (sender.IsMe && args.SData.Name == "KalistaExpungeWrapper")
                {
                    Orbwalking.ResetAutoAttackTimer();
                }
            };
            Spellbook.OnCastSpell += (sender, args) =>
            {
                if (sender.Owner.IsMe && args.Slot == SpellSlot.Q && ObjectManager.Player.IsDashing())
                {
                    args.Process = false;
                }
            };
        }

        /// <summary>
        ///     This is where jeff creates his first Menu in a long time
        /// </summary>
        private void CreateMenu()
        {
            Menu = new Menu("iKalista: Reborn", "com.ikalista", true);

            var targetSelector = new Menu("iKalista: Reborn - Target Selector", "com.ikalista.ts");
            TargetSelector.AddToMenu(targetSelector);
            Menu.AddSubMenu(targetSelector);

            var orbwalkerMenu = new Menu("iKalista: Reborn - Orbwalker", "com.ikalista.orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            var comboMenu = new Menu("iKalista: Reborn - Combo", "com.ikalista.combo");
            {
                comboMenu.AddBool("com.ikalista.combo.useQ", "Use Q", true);
                comboMenu.AddBool("com.ikalista.combo.useE", "Use E", true);
                comboMenu.AddSlider("com.ikalista.combo.stacks", "Rend at X stacks", 10, 1, 20);
                Menu.AddSubMenu(comboMenu);
            }

            var mixedMenu = new Menu("iKalista: Reborn - Mixed", "com.ikalista.mixed");
            {
                mixedMenu.AddBool("com.ikalista.mixed.useQ", "Use Q", true);
                mixedMenu.AddBool("com.ikalista.mixed.useE", "Use E", true);
                mixedMenu.AddSlider("com.ikalista.mixed.stacks", "Rend at X stacks", 10, 1, 20);
                Menu.AddSubMenu(mixedMenu);
            }

            var laneclearMenu = new Menu("iKalista: Reborn - Laneclear", "com.ikalista.laneclear");
            {
                laneclearMenu.AddBool("com.ikalista.laneclear.useQ", "Use Q", true);
                laneclearMenu.AddSlider("com.ikalista.laneclear.qMinions", "Min Minions for Q", 3, 1, 10);
                laneclearMenu.AddBool("com.ikalista.laneclear.useE", "Use E", true);
                laneclearMenu.AddSlider("com.ikalista.laneclear.eMinions", "Min Minions for E", 5, 1, 10);
                Menu.AddSubMenu(laneclearMenu);
            }

            var jungleStealMenu = new Menu("iKalista: Reborn - Jungle Steal", "com.ikalista.jungleSteal");
            {
                jungleStealMenu.AddStringList("com.ikalista.jungleSteal.mode", "Mode",
                    new[] {"Objectives", "All Mobs", "Both"});
                Menu.AddSubMenu(jungleStealMenu);
            }

            var modulesMenu = new Menu("iKalista: Reborn - Modules", "com.ikalista.modules");
            {
                foreach (var module in Modules)
                {
                    modulesMenu.AddBool("com.ikalista.modules." + module.GetName().ToLowerInvariant(),
                        "" + module.GetName(), true);
                }
                Menu.AddSubMenu(modulesMenu);
            }

            Menu.AddToMainMenu();
        }

        private void LoadModules()
        {
            foreach (var module in Modules.Where(x => x.ShouldGetExecuted()))
            {
                try
                {
                    module.OnLoad();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error loading module: " + module.GetName() + " Exception: " + e);
                }
            }
        }

        /// <summary>
        ///     My names definatly jeffery.
        /// </summary>
        /// <param name="args">even more gay</param>
        private void OnDraw(EventArgs args)
        {
        }

        /// <summary>
        ///     My Names Jeff
        /// </summary>
        /// <param name="args">gay</param>
        private void OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    OnMixed();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    OnLaneclear();
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            foreach (var module in Modules.Where(x => x.ShouldGetExecuted()))
            {
                module.OnExecute();
            }
        }

        private void OnCombo()
        {
            if (!SpellManager.Spell[SpellSlot.Q].IsReady() || !Menu.Item("com.ikalista.combo.useQ").GetValue<bool>())
                return;

            var target = TargetSelector.GetTarget(SpellManager.Spell[SpellSlot.Q].Range,
                TargetSelector.DamageType.Physical);
            var prediction = SpellManager.Spell[SpellSlot.Q].GetSPrediction(target);
            if (prediction.HitChance >= HitChance.High &&
                target.IsValidTarget(SpellManager.Spell[SpellSlot.Q].Range) && !ObjectManager.Player.IsDashing() &&
                !ObjectManager.Player.IsWindingUp)
            {
                SpellManager.Spell[SpellSlot.Q].Cast(prediction.CastPosition);
            }
        }

        private void OnMixed()
        {
            if (SpellManager.Spell[SpellSlot.Q].IsReady() && Menu.Item("com.ikalista.mixed.useQ").GetValue<bool>())
            {
                var target = TargetSelector.GetTarget(SpellManager.Spell[SpellSlot.Q].Range,
                    TargetSelector.DamageType.Physical);
                var prediction = SpellManager.Spell[SpellSlot.Q].GetSPrediction(target);
                if (prediction.HitChance >= HitChance.High &&
                    target.IsValidTarget(SpellManager.Spell[SpellSlot.Q].Range))
                {
                    SpellManager.Spell[SpellSlot.Q].Cast(prediction.CastPosition);
                }
            }

            if (SpellManager.Spell[SpellSlot.E].IsReady() && Menu.Item("com.ikalista.mixed.useE").GetValue<bool>())
            {
                foreach (
                    var source in
                        HeroManager.Enemies.Where(
                            x => x.IsValid && x.HasRendBuff() && SpellManager.Spell[SpellSlot.E].IsInRange(x)))
                {
                    if (source.IsRendKillable() ||
                        source.GetRendBuffCount() >= Menu.Item("com.ikalista.mixed.stacks").GetValue<Slider>().Value)
                    {
                        SpellManager.Spell[SpellSlot.E].Cast();
                    }
                }
            }
        }

        private void OnLaneclear()
        {
            if (Menu.Item("com.ikalista.laneclear.useQ").GetValue<bool>())
            {
                var minions = MinionManager.GetMinions(SpellManager.Spell[SpellSlot.Q].Range).ToList();
                if (minions.Count < 0)
                    return;

                foreach (var minion in minions.Where(x => x.Health <= SpellManager.Spell[SpellSlot.Q].GetDamage(x)))
                {
                    var killableMinions = Helper.GetCollisionMinions(ObjectManager.Player,
                        ObjectManager.Player.ServerPosition.Extend(
                            minion.ServerPosition,
                            SpellManager.Spell[SpellSlot.Q].Range))
                        .Count(
                            collisionMinion =>
                                collisionMinion.Health
                                <= ObjectManager.Player.GetSpellDamage(collisionMinion, SpellSlot.Q));

                    if (killableMinions >= Menu.Item("com.ikalista.laneclear.qMinions").GetValue<Slider>().Value)
                    {
                        SpellManager.Spell[SpellSlot.Q].Cast(minion.ServerPosition);
                    }
                }
            }
            if (Menu.Item("com.ikalista.laneclear.useE").GetValue<bool>())
            {
                var minions = MinionManager.GetMinions(SpellManager.Spell[SpellSlot.E].Range).ToList();
                if (minions.Count < 0)
                    return;

                var count =
                    minions.Count(
                        x => SpellManager.Spell[SpellSlot.E].CanCast(x) && x.IsMobKillable());

                if (count >= Menu.Item("com.ikalista.laneclear.eMinions").GetValue<Slider>().Value &&
                    !ObjectManager.Player.HasBuff("summonerexhaust"))
                {
                    SpellManager.Spell[SpellSlot.E].Cast();
                }
            }
        }
    }
}