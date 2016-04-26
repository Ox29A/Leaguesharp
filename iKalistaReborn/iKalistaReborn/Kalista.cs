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
using System.Drawing;

/*
    TODO: 

    1. it doesnt rend the big super minions would be helpful for laneclear later

    2. sometimes it doesnt rend jungle mobs for me checked everything it only rend drake and baron o.O but only sometimes

    3. it uses rend into kindred ult cost my last game... :-/

    4. color change for e damage didnt work

    5. rend minions u cant get with aa

*/

namespace iKalistaReborn
{
    internal class Kalista
    {
        public static Menu Menu;

        public static List<string> JungleMinions = new List<string>
        {
            "SRU_Baron",
            "SRU_Blue",
            "Sru_Crab",
            "SRU_Dragon",
            "SRU_Gromp",
            "SRU_Krug",
            "SRU_Murkwolf",
            "SRU_Razorbeak",
            "SRU_Red"
        };

        /// <summary>
        ///     The Modules
        /// </summary>
        public static readonly List<IModule> Modules = new List<IModule>
        {
            new AutoRendModule(),
            new JungleStealModule(),
            new AutoEModule(),
            new AutoELeavingModule()
        };

        public static Orbwalking.Orbwalker Orbwalker;

        public Kalista()
        {
            CreateMenu();
            LoadModules();
            CustomDamageIndicator.Initialize(Helper.GetRendDamage);
            SPrediction.Prediction.Initialize(Menu);
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            Spellbook.OnCastSpell += (sender, args) =>
            {
                if (sender.Owner.IsMe && args.Slot == SpellSlot.Q && ObjectManager.Player.IsDashing())
                {
                    args.Process = false;
                }
            };
            Orbwalker.RegisterCustomMode("com.ikalista.flee", "Flee", "V".ToCharArray()[0]);
            Orbwalking.OnNonKillableMinion += minion =>
            {
                var killableMinion = minion as Obj_AI_Base;
                if (killableMinion == null || !SpellManager.Spell[SpellSlot.E].IsReady()
                    || ObjectManager.Player.HasBuff("summonerexhaust") || !killableMinion.HasRendBuff())
                {
                    return;
                }

                if (Menu.Item("com.ikalista.laneclear.useEUnkillable").GetValue<bool>() &&
                    killableMinion.IsMobKillable())
                {
                    SpellManager.Spell[SpellSlot.E].Cast();
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
                comboMenu.AddText("--", "------------------");
                comboMenu.AddBool("com.ikalista.combo.useE", "Use E", true);
                comboMenu.AddSlider("com.ikalista.combo.stacks", "Rend at X stacks", 10, 1, 20);
                comboMenu.AddBool("com.ikalista.combo.eLeaving", "Use E Leaving", true);
                comboMenu.AddSlider("com.ikalista.combo.ePercent", "Min Percent for E Leaving", 50, 10, 100);
                comboMenu.AddBool("com.ikalista.combo.saveMana", "Save Mana for E", true);
                comboMenu.AddBool("com.ikalista.combo.autoE", "Auto E Minion > Champion", true);
                comboMenu.AddBool("com.ikalista.combo.orbwalkMinions", "Orbwalk Minions in combo", true);
                comboMenu.AddText("---", "------------------");
                comboMenu.AddBool("com.ikalista.combo.saveAlly", "Save Ally With R", true);
                comboMenu.AddBool("com.ikalista.combo.balista", "Use Balista", true);
                comboMenu.AddSlider("com.ikalista.combo.allyPercent", "Min Health % for Ally", 20, 10, 100);
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
                laneclearMenu.AddBool("com.ikalista.laneclear.useEUnkillable", "E Unkillable Minions", true);
                Menu.AddSubMenu(laneclearMenu);
            }

            var jungleStealMenu = new Menu("iKalista: Reborn - Jungle Steal", "com.ikalista.jungleSteal");
            {
                foreach (var minion in JungleMinions)
                {
                    jungleStealMenu.AddBool(minion, minion, true);
                }
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

            var drawingMenu = new Menu("iKalista: Reborn - Drawing", "com.ikalista.drawing");
            {
                drawingMenu.AddBool("com.ikalista.drawing.spellRanges", "Draw Spell Ranges", true);
                drawingMenu.AddItem(
                    new MenuItem("com.ikalista.drawing.eDamage", "Draw E Damage").SetValue(new Circle(true,
                        Color.DarkOliveGreen)));
                drawingMenu.AddItem(
                    new MenuItem("com.ikalista.drawing.damagePercent", "Draw Percent Damage").SetValue(new Circle(true,
                        Color.DarkOliveGreen)));
                Menu.AddSubMenu(drawingMenu);
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
            if (Menu.Item("com.ikalista.drawing.spellRanges").GetValue<bool>())
            {
                foreach (var spell in SpellManager.Spell.Values)
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, Color.DarkOliveGreen);
                }
            }

            if (Menu.Item("com.ikalista.drawing.damagePercent").GetValue<Circle>().Active)
            {
                foreach (var source in HeroManager.Enemies.Where(x => ObjectManager.Player.Distance(x) <= 2000f && !x.IsDead))
                {
                    var currentPercentage = Math.Round(Helper.GetRendDamage(source)*100/source.GetHealthWithShield(), 2);

                    Drawing.DrawText(
                        Drawing.WorldToScreen(source.Position)[0],
                        Drawing.WorldToScreen(source.Position)[1],
                        currentPercentage >= 100
                            ? Menu.Item("com.kalista.drawing.damagePercent").GetValue<Circle>().Color
                            : Color.White,
                        currentPercentage >= 100
                            ? "Killable With E"
                            : "Current Damage: " + currentPercentage + "%");
                }
            }

            CustomDamageIndicator.DrawingColor = Menu.Item("com.ikalista.drawing.eDamage").GetValue<Circle>().Color;
            CustomDamageIndicator.Enabled = Menu.Item("com.ikalista.drawing.eDamage").GetValue<Circle>().Active;
        }

        /// <summary>
        ///     The on process spell function
        /// </summary>
        /// <param name="sender">
        ///     The Spell Sender
        /// </param>
        /// <param name="args">
        ///     The Arguments
        /// </param>
        private void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "KalistaExpungeWrapper")
            {
                Orbwalking.ResetAutoAttackTimer();
            }

            if (sender.Type == GameObjectType.obj_AI_Hero && sender.IsEnemy && args.Target != null &&
                Menu.Item("com.ikalista.combo.saveAlly").GetValue<bool>())
            {
                var soulboundhero =
                    HeroManager.Allies.FirstOrDefault(
                        hero =>
                            hero.HasBuff("kalistacoopstrikeally") && args.Target.NetworkId == hero.NetworkId);

                if (soulboundhero != null &&
                    soulboundhero.HealthPercent < Menu.Item("com.ikalista.combo.allyPercent").GetValue<Slider>().Value)
                {
                    SpellManager.Spell[SpellSlot.R].Cast();
                }
            }
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
                case Orbwalking.OrbwalkingMode.CustomMode:
                    OnFlee();
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //BALISTA
            if (Menu.Item("com.ikalista.combo.balista").GetValue<bool>() && SpellManager.Spell[SpellSlot.R].IsReady())
            {
                var soulboundhero = HeroManager.Allies.FirstOrDefault(x => x.HasBuff("kalistacoopstrikeally"));
                if (soulboundhero?.ChampionName == "Blitzcrank")
                {
                    foreach (
                        var unit in
                            HeroManager.Enemies
                                .Where(
                                    h => h.IsHPBarRendered &&
                                         h.Distance(ObjectManager.Player.ServerPosition) > 700 &&
                                         h.Distance(ObjectManager.Player.ServerPosition) < 1400)
                        )
                    {
                        if (unit.HasBuff("rocketgrab2"))
                        {
                            SpellManager.Spell[SpellSlot.R].Cast();
                        }
                    }
                }
            }

            foreach (var module in Modules.Where(x => x.ShouldGetExecuted()))
            {
                module.OnExecute();
            }
        }

        private void OnFlee()
        {
            var bestTarget =
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(x => x.IsEnemy && ObjectManager.Player.Distance(x) <= Orbwalking.GetRealAutoAttackRange(x))
                    .OrderBy(x => ObjectManager.Player.Distance(x))
                    .FirstOrDefault();

            Orbwalking.Orbwalk(bestTarget, Game.CursorPos);

            //TODO wall flee
        }

        private void OnCombo()
        {
            if (Menu.Item("com.ikalista.combo.orbwalkMinions").GetValue<bool>())
            {
                var targets =
                    HeroManager.Enemies.Where(
                        x =>
                            ObjectManager.Player.Distance(x) <= SpellManager.Spell[SpellSlot.E].Range*2 &&
                            x.IsValidTarget(SpellManager.Spell[SpellSlot.E].Range*2));

                if (targets.Count(x => ObjectManager.Player.Distance(x) < Orbwalking.GetRealAutoAttackRange(x)) == 0)
                {
                    var minion =
                        ObjectManager.Get<Obj_AI_Minion>()
                            .Where(
                                x =>
                                    ObjectManager.Player.Distance(x) <= Orbwalking.GetRealAutoAttackRange(x) &&
                                    x.IsEnemy)
                            .OrderBy(x => x.Health)
                            .FirstOrDefault();
                    if (minion != null)
                    {
                        Orbwalking.Orbwalk(minion, Game.CursorPos);
                    }
                }
            }

            if (!SpellManager.Spell[SpellSlot.Q].IsReady() || !Menu.Item("com.ikalista.combo.useQ").GetValue<bool>() ||
                ObjectManager.Player.Mana <
                SpellManager.Spell[SpellSlot.Q].ManaCost + SpellManager.Spell[SpellSlot.E].ManaCost)
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