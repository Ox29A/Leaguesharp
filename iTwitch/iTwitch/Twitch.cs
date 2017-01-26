using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DZLib.MenuExtensions;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = SharpDX.Color;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace iTwitch
{
    public class Twitch
    {
        #region Static Fields

        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        public static readonly Dictionary<SpellSlot, Spell> Spells = new Dictionary<SpellSlot, Spell>
        {
            {SpellSlot.Q, new Spell(SpellSlot.Q)},
            {SpellSlot.W, new Spell(SpellSlot.W, 950f)},
            {SpellSlot.E, new Spell(SpellSlot.E, 1100)},
            {SpellSlot.R, new Spell(SpellSlot.R)}
        };

        #endregion

        #region Fields

        public static Menu Menu;

        private static Orbwalking.Orbwalker _orbwalker;

        #endregion

        #region Public Methods and Operators

        public void LoadMenu()
        {
            Menu = new Menu("iTwitch 2.0", "com.itwitch", true).SetFontStyle(FontStyle.Bold, Color.AliceBlue);

            var owMenu = new Menu(":: Orbwalker", "com.itwitch.orbwalker");
            {
                _orbwalker = new Orbwalking.Orbwalker(owMenu);
                Menu.AddSubMenu(owMenu);
            }

            var comboMenu = new Menu(":: iTwitch 2.0 - Combo Options", "com.itwitch.combo");
            {
                comboMenu.AddBool("com.itwitch.combo.useQ", "Use Q", true);
                comboMenu.AddBool("com.itwitch.combo.useW", "Use W", true);
                comboMenu.AddBool("com.itwitch.combo.useEKillable", "Use E Killable", true);
                Menu.AddSubMenu(comboMenu);
            }

            var harassMenu = new Menu(":: iTwitch 2.0 - Harass Options", "com.itwitch.harass");
            {
                harassMenu.AddBool("com.itwitch.harass.useW", "Use W");
                harassMenu.AddBool("com.itwitch.harass.useEKillable", "Use E", true);
                Menu.AddSubMenu(harassMenu);
            }

            var miscMenu = new Menu(":: iTwitch 2.0 - Misc Options", "com.itwitch.misc");
            {
                miscMenu.AddBool("com.itwitch.misc.autoYo", "Youmuus with R", true);
                miscMenu.AddBool("com.itwitch.misc.noWTurret", "Don't W Under Tower", true);
                miscMenu.AddSlider("com.itwitch.misc.noWAA", "No W if x aa can kill", 2, 0, 10);
                miscMenu.AddBool("com.itwitch.misc.ebeforedeath", "E Before Death", true);
                miscMenu.AddBool("com.itwitch.misc.saveManaE", "Save Mana for E", true);
                miscMenu.AddBool("com.itwitch.misc.EAAQ", "E AA Q")
                    .SetTooltip("Will cast E if killable by E + AA then Q");
                miscMenu.AddKeybind(
                    "com.itwitch.misc.recall",
                    "Stealth Recall",
                    new Tuple<uint, KeyBindType>("T".ToCharArray()[0], KeyBindType.Press));
                Menu.AddSubMenu(miscMenu);
            }

            var drawingMenu = new Menu(":: iTwitch 2.0 - Drawing Options", "com.itwitch.drawing");
            {
                drawingMenu.AddBool("com.itwitch.drawing.drawERange", "Draw E Range", true);
                drawingMenu.AddBool("com.itwitch.drawing.drawRRange", "Draw R Range", true);
                drawingMenu.AddBool("com.itwitch.drawing.drawQTime", "Draw Q Time", true);
                drawingMenu.AddBool("com.itwitch.drawing.drawEStacks", "Draw E Stacks", true);
                drawingMenu.AddBool("com.itwitch.drawing.drawEStackT", "Draw E Stack Time", true);
                drawingMenu.AddBool("com.itwitch.drawing.drawRTime", "Draw R Time", true);
                drawingMenu.AddItem(
                    new MenuItem("com.itwitch.drawing.eDamage", "Draw E Damage on Enemies").SetValue(
                        new Circle(true, System.Drawing.Color.DarkOliveGreen)));
                Menu.AddSubMenu(drawingMenu);
            }

            Menu.AddToMainMenu();
        }

        public void LoadSpells()
        {
            Spells[SpellSlot.W].SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotCircle);
        }

        public void OnCombo()
        {
            if (!Menu.Item("com.itwitch.combo.useQ").GetValue<bool>() || !Spells[SpellSlot.Q].IsReady()) return;
            if (Menu.Item("com.itwitch.misc.saveManaE").GetValue<bool>()
                && ObjectManager.Player.Mana >= Spells[SpellSlot.E].ManaCost + Spells[SpellSlot.Q].ManaCost)
                Spells[SpellSlot.Q].Cast();
        }

        public void OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Twitch") return;

            CustomDamageIndicator.Initialize(Extensions.GetPoisonDamage);

            LoadSpells();
            LoadMenu();

            Spellbook.OnCastSpell += (sender, eventArgs) =>
            {
                if (eventArgs.Slot == SpellSlot.Recall && Spells[SpellSlot.Q].IsReady()
                    && Menu.Item("com.itwitch.misc.recall").GetValue<KeyBind>().Active)
                {
                    Spells[SpellSlot.Q].Cast();
                    Utility.DelayAction.Add(
                        (int) (Spells[SpellSlot.Q].Delay + 300),
                        () => ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Recall));
                    eventArgs.Process = false;
                    return;
                }

                if (eventArgs.Slot == SpellSlot.R && Menu.Item("com.itwitch.misc.autoYo").GetValue<bool>())
                {
                    if (!HeroManager.Enemies.Any(x => ObjectManager.Player.Distance(x) <= Spells[SpellSlot.R].Range))
                        return;

                    if (Items.HasItem(ItemData.Youmuus_Ghostblade.Id))
                        Items.UseItem(ItemData.Youmuus_Ghostblade.Id);
                }

                if (Menu.Item("com.itwitch.misc.saveManaE").GetValue<bool>() && eventArgs.Slot == SpellSlot.W)
                    if (ObjectManager.Player.Mana <= Spells[SpellSlot.E].ManaCost + 10)
                        eventArgs.Process = false;
            };

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalking.OnAttack += AfterAttack;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
        }

        private static void AfterAttack(AttackableUnit unit, AttackableUnit target1)
        {
            var target = target1 as Obj_AI_Hero;
            if (target == null || !unit.IsMe || !target.IsValidTarget(Spells[SpellSlot.W].Range) ||
                target.IsInvulnerable)
                return;

            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (!Menu.Item("com.itwitch.combo.useW").GetValue<bool>() || !Spells[SpellSlot.W].IsReady())
                        return;

                    if (Menu.Item("com.itwitch.misc.saveManaE").GetValue<bool>()
                        && ObjectManager.Player.Mana <= Spells[SpellSlot.E].ManaCost + Spells[SpellSlot.W].ManaCost)
                        return;

                    if (Menu.Item("com.itwitch.misc.noWTurret").GetValue<bool>() &&
                        ObjectManager.Player.UnderTurret(true))
                        return;

                    if (target.Health
                        < ObjectManager.Player.GetAutoAttackDamage(target, true)
                        * Menu.Item("com.itwitch.misc.noWAA").GetValue<Slider>().Value) return;

                    if (target.IsValidTarget(Spells[SpellSlot.W].Range)
                        && !ObjectManager.Player.HasBuff("TwitchHideInShadows"))
                    {
                        var prediction = Spells[SpellSlot.W].GetPrediction(target);
                        if (prediction.Hitchance >= HitChance.High)
                            Spells[SpellSlot.W].Cast(prediction.CastPosition);
                    }
                    break;
            }
        }

        public void OnHarass()
        {
            if (Menu.Item("com.itwitch.harass.useW").GetValue<bool>() && Spells[SpellSlot.W].IsReady())
            {
                var wTarget = TargetSelector.GetTarget(Spells[SpellSlot.W].Range, TargetSelector.DamageType.Physical);
                if (wTarget.IsValidTarget(Spells[SpellSlot.W].Range))
                {
                    var prediction = Spells[SpellSlot.W].GetPrediction(wTarget);
                    if (prediction.Hitchance >= HitChance.High)
                        Spells[SpellSlot.W].Cast(prediction.CastPosition);
                }
            }
        }

        #endregion

        #region Methods

        private void OnDraw(EventArgs args)
        {
            CustomDamageIndicator.Enabled = Menu.Item("com.itwitch.drawing.eDamage").GetValue<Circle>().Active;

            if (Menu.Item("com.itwitch.drawing.drawRRange").GetValue<bool>())
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Spells[SpellSlot.R].Range,
                    System.Drawing.Color.BlueViolet);
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 850, System.Drawing.Color.BlueViolet);
            }

            if (Menu.Item("com.itwitch.drawing.drawERange").GetValue<bool>())
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Spells[SpellSlot.E].Range,
                    System.Drawing.Color.BlueViolet);

            if (Menu.Item("com.itwitch.drawing.drawQTime").GetValue<bool>()
                && ObjectManager.Player.HasBuff("TwitchHideInShadows"))
            {
                var position = new Vector3(
                    ObjectManager.Player.Position.X,
                    ObjectManager.Player.Position.Y - 30,
                    ObjectManager.Player.Position.Z);
                position.DrawTextOnScreen(
                    "Stealth:  " + $"{ObjectManager.Player.GetRemainingBuffTime("TwitchHideInShadows"):0.0}",
                    System.Drawing.Color.AntiqueWhite);
            }

            if (Menu.Item("com.itwitch.drawing.drawRTime").GetValue<bool>()
                && ObjectManager.Player.HasBuff("TwitchFullAutomatic"))
                ObjectManager.Player.Position.DrawTextOnScreen(
                    "Ultimate:  " + $"{ObjectManager.Player.GetRemainingBuffTime("TwitchFullAutomatic"):0.0}",
                    System.Drawing.Color.AntiqueWhite);

            if (Menu.Item("com.itwitch.drawing.drawEStacks").GetValue<bool>())
                foreach (var source in
                    HeroManager.Enemies.Where(x => x.HasBuff("TwitchDeadlyVenom") && !x.IsDead && x.IsVisible))
                {
                    var position = new Vector3(source.Position.X, source.Position.Y + 10, source.Position.Z);
                    position.DrawTextOnScreen($"{"Stacks: " + source.GetPoisonStacks()}",
                        System.Drawing.Color.AntiqueWhite);
                }

            if (Menu.Item("com.itwitch.drawing.drawEStackT").GetValue<bool>())
                foreach (var source in
                    HeroManager.Enemies.Where(x => x.HasBuff("TwitchDeadlyVenom") && !x.IsDead && x.IsVisible))
                {
                    var position = new Vector3(source.Position.X, source.Position.Y - 30, source.Position.Z);
                    position.DrawTextOnScreen(
                        "Stack Timer:  " + $"{source.GetRemainingBuffTime("TwitchDeadlyVenom"):0.0}",
                        System.Drawing.Color.AntiqueWhite);
                }
        }

        private void OnUpdate(EventArgs args)
        {
            if (Menu.Item("com.itwitch.misc.recall").GetValue<KeyBind>().Active)
                ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Recall);

            if (Menu.Item("com.itwitch.misc.ebeforedeath").GetValue<bool>() && Spells[SpellSlot.E].IsReady()
                && HealthPrediction.GetHealthPrediction(ObjectManager.Player, (int) (Game.Time + 1000.0)) <= 50.0f)
                Spells[SpellSlot.E].Cast();

            if (Menu.Item("com.itwitch.combo.useEKillable").GetValue<bool>() && Spells[SpellSlot.E].IsReady())
                if (HeroManager.Enemies.Any(x => x.IsPoisonKillable() && x.IsValidTarget(Spells[SpellSlot.E].Range)))
                    Spells[SpellSlot.E].Cast();

            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;

                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;
            }
        }

        #endregion
    }
}