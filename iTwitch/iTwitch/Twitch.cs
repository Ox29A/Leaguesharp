using System;
using System.Collections.Generic;
using System.Linq;
using DZLib.MenuExtensions;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace iTwitch
{
    public class Twitch
    {
        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        public static readonly Dictionary<SpellSlot, Spell> Spells = new Dictionary<SpellSlot, Spell>
        {
            {SpellSlot.Q, new Spell(SpellSlot.Q)},
            {SpellSlot.W, new Spell(SpellSlot.W, 950f)},
            {SpellSlot.E, new Spell(SpellSlot.E, 1200f)}
        };

        private Menu _menu;
        private Orbwalking.Orbwalker _orbwalker;

        public void OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Twitch")
                return;

            LoadSpells();
            LoadMenu();

            Spellbook.OnCastSpell += (sender, eventArgs) =>
            {
                if (eventArgs.Slot == SpellSlot.Recall && Spells[SpellSlot.Q].IsReady() && _menu.Item("com.itwitch.misc.recall").GetValue<KeyBind>().Active)
                {
                    Spells[SpellSlot.Q].Cast();
                    Utility.DelayAction.Add((int) (Spells[SpellSlot.Q].Delay + 300),
                        () => ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Recall));
                    eventArgs.Process = false;
                    return;
                }

                if (eventArgs.Slot == SpellSlot.R && _menu.Item("com.itwitch.misc.autoYo").GetValue<bool>())
                {
                    if (!HeroManager.Enemies.Any(x => ObjectManager.Player.Distance(x) <= Spells[SpellSlot.R].Range))
                        return;

                    if (Items.HasItem(ItemData.Youmuus_Ghostblade.Id))
                    {
                        Items.UseItem(ItemData.Youmuus_Ghostblade.Id);
                    }
                }
                if (_menu.Item("com.itwitch.misc.saveManaE").GetValue<bool>() && eventArgs.Slot == SpellSlot.W)
                {
                    if (ObjectManager.Player.Mana <= Spells[SpellSlot.E].ManaCost + 10)
                    {
                        eventArgs.Process = false;
                    }
                }
            };

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        public void LoadMenu()
        {
            _menu = new Menu("iTwitch 2.0 - Hawk Mode", "com.itwitch", true);

            var owMenu = new Menu(":: Orbwalker", "com.itwitch.orbwalker");
            {
                _orbwalker = new Orbwalking.Orbwalker(owMenu);
                _menu.AddSubMenu(owMenu);
            }

            var comboMenu = new Menu(":: iTwith 2.0 - Combo Options", "com.itwitch.combo");
            {
                comboMenu.AddBool("com.itwitch.combo.useW", "Use W", true);
                comboMenu.AddBool("com.itwitch.combo.useEKillable", "Use E Killable", true);
                _menu.AddSubMenu(comboMenu);
            }

            var harassMenu = new Menu(":: iTwith 2.0 - Harass Options", "com.itwitch.harass");
            {
                harassMenu.AddBool("com.itwitch.harass.useW", "Use W");
                harassMenu.AddBool("com.itwitch.harass.useEKillable", "Use E", true);
                _menu.AddSubMenu(harassMenu);
            }

            var miscMenu = new Menu(":: iTwitch 2.0 - Misc Options", "com.itwitch.misc");
            {
                miscMenu.AddBool("com.itwitch.misc.autoYo", "Yomuus with R", true);
                miscMenu.AddBool("com.itwitch.misc.saveManaE", "Save Mana for E", true);
                miscMenu.AddKeybind("com.itwitch.misc.recall", "Stealth Recall", new Tuple<uint, KeyBindType>("T".ToCharArray()[0], KeyBindType.Press));
                _menu.AddSubMenu(miscMenu);
            }

            var drawingMenu = new Menu(":: iTwith 2.0 - Drawing Options", "com.itwitch.drawing");
            {
                drawingMenu.AddBool("com.itwitch.drawing.drawQTime", "Draw Q Time", true);
                drawingMenu.AddBool("com.itwitch.drawing.drawEStacks", "Draw E Stacks", true);
                drawingMenu.AddBool("com.itwitch.drawing.drawEStackT", "Draw E Stack Time", true);
                drawingMenu.AddBool("com.itwitch.drawing.drawRTime", "Draw R Time", true);
                _menu.AddSubMenu(drawingMenu);
            }

            _menu.AddToMainMenu();
        }

        public void LoadSpells()
        {
            Spells[SpellSlot.W].SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotCircle);
        }

        private void OnUpdate(EventArgs args)
        {

            if (_menu.Item("com.itwitch.misc.recall").GetValue<KeyBind>().Active)
            {
                ObjectManager.Player.Spellbook.CastSpell(SpellSlot.Recall);
            }

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

        private void OnDraw(EventArgs args)
        {
            if (_menu.Item("com.itwitch.drawing.drawQTime").GetValue<bool>() &&
                ObjectManager.Player.HasBuff("TwitchHideInShadows"))
            {
                var position = new Vector3(ObjectManager.Player.Position.X, ObjectManager.Player.Position.Y - 30,
                    ObjectManager.Player.Position.Z);
                position.DrawTextOnScreen(
                    "Stealth:  " + $"{ObjectManager.Player.GetRemainingBuffTime("TwitchHideInShadows"):0.0}",
                    System.Drawing.Color.AntiqueWhite);
            }

            if (_menu.Item("com.itwitch.drawing.drawRTime").GetValue<bool>() &&
                ObjectManager.Player.HasBuff("TwitchFullAutomatic"))
            {
                ObjectManager.Player.Position.DrawTextOnScreen(
                    "Ultimate:  " + $"{ObjectManager.Player.GetRemainingBuffTime("TwitchFullAutomatic"):0.0}",
                    System.Drawing.Color.AntiqueWhite);
            }

            if (_menu.Item("com.itwitch.drawing.drawEStacks").GetValue<bool>())
            {
                foreach (
                    var source in
                        HeroManager.Enemies.Where(
                            x => x.HasBuff("TwitchDeadlyVenom") && !x.IsDead && x.IsVisible))
                {
                    var position = new Vector3(source.Position.X, source.Position.Y + 10, source.Position.Z);
                    position.DrawTextOnScreen($"{"Stacks: " + source.GetPoisonStacks()}",
                        System.Drawing.Color.AntiqueWhite);
                }
            }
            if (_menu.Item("com.itwitch.drawing.drawEStackT").GetValue<bool>())
            {
                foreach (
                    var source in
                        HeroManager.Enemies.Where(
                            x => x.HasBuff("TwitchDeadlyVenom") && !x.IsDead && x.IsVisible))
                {
                    var position = new Vector3(source.Position.X, source.Position.Y - 30, source.Position.Z);
                    position.DrawTextOnScreen(
                    "Stack Timer:  " + $"{source.GetRemainingBuffTime("TwitchDeadlyVenom"):0.0}",
                    System.Drawing.Color.AntiqueWhite);
                }
            }
        }

        public void OnCombo()
        {
            if (_menu.Item("com.itwitch.combo.useEKillable").GetValue<bool>() && Spells[SpellSlot.E].IsReady())
            {
                var killableTarget =
                    HeroManager.Enemies.FirstOrDefault(
                        x =>
                            x.IsValidTarget(Spells[SpellSlot.E].Range) && Spells[SpellSlot.E].IsInRange(x)
                            && x.IsPoisonKillable());
                if (killableTarget != null)
                {
                    Spells[SpellSlot.E].Cast();
                }
            }
            if (_menu.Item("com.itwitch.combo.useW").GetValue<bool>() && Spells[SpellSlot.W].IsReady())
            {
                var wTarget = TargetSelector.GetTarget(Spells[SpellSlot.W].Range, TargetSelector.DamageType.Physical);
                if (wTarget.IsValidTarget(Spells[SpellSlot.W].Range))
                {
                    var prediction = Spells[SpellSlot.W].GetPrediction(wTarget);
                    if (prediction.Hitchance >= HitChance.High)
                    {
                        Spells[SpellSlot.W].Cast(prediction.CastPosition);
                    }
                }
            }
        }


        public void OnHarass()
        {
            if (_menu.Item("com.itwitch.harass.useEKillable").GetValue<bool>() && Spells[SpellSlot.E].IsReady())
            {
                var target =
                    HeroManager.Enemies.FirstOrDefault(
                        x =>
                            x.IsValidTarget(Spells[SpellSlot.E].Range) && Spells[SpellSlot.E].IsInRange(x) &&
                            x.IsPoisonKillable());
                if (target != null)
                {
                    Spells[SpellSlot.E].Cast();
                }
            }
            if (_menu.Item("com.itwitch.harass.useW").GetValue<bool>() && Spells[SpellSlot.W].IsReady())
            {
                var wTarget = TargetSelector.GetTarget(Spells[SpellSlot.W].Range, TargetSelector.DamageType.Physical);
                if (wTarget.IsValidTarget(Spells[SpellSlot.W].Range))
                {
                    var prediction = Spells[SpellSlot.W].GetPrediction(wTarget);
                    if (prediction.Hitchance >= HitChance.High)
                    {
                        Spells[SpellSlot.W].Cast(prediction.CastPosition);
                    }
                }
            }
        }
    }
}