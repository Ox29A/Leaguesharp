using System;
using System.Collections.Generic;
using System.Linq;
using DZLib.MenuExtensions;
using LeagueSharp;
using LeagueSharp.Common;
using ItemData = LeagueSharp.Common.Data.ItemData;

namespace iTwitch
{
    public class Twitch
    {
        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        private readonly Dictionary<SpellSlot, Spell> _spells = new Dictionary<SpellSlot, Spell>
        {
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
                if (eventArgs.Slot == SpellSlot.R && _menu.Item("com.itwitch.misc.autoYo").GetValue<bool>())
                {
                    if (ItemData.Youmuus_Ghostblade.GetItem().IsReady())
                    {
                        ItemData.Youmuus_Ghostblade.GetItem().Cast();
                    }
                }
                if (_menu.Item("com.itwitch.misc.saveManaE").GetValue<bool>() && eventArgs.Slot == SpellSlot.W)
                {
                    if (ObjectManager.Player.Mana <= _spells[SpellSlot.E].ManaCost + 50)
                    {
                        eventArgs.Process = false;
                    }
                }

            };

            Game.OnUpdate += OnUpdate;
        }

        public void LoadMenu()
        {
            _menu = new Menu(":: iTwitch 2.0", "com.itwitch", true);

            var owMenu = new Menu("[Ez] Orbwalker", "ezreal.orbwalker");
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
                harassMenu.AddSlider("com.itwitch.harass.eStacks", "E at x Stacks", 10, 1, 20);
                _menu.AddSubMenu(harassMenu);
            }

            var miscMenu = new Menu(":: iTwitch 2.0 - Misc Options", "com.itwitch.misc");
            {
                miscMenu.AddBool("com.itwitch.misc.autoYo", "Yomuus with R", true);

                miscMenu.AddBool("com.itwitch.misc.saveManaE", "Save Mana for E", true);
                _menu.AddSubMenu(miscMenu);
            }

            _menu.AddToMainMenu();
        }

        public void LoadSpells()
        {
            _spells[SpellSlot.W].SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotCircle);
        }

        private void OnUpdate(EventArgs args)
        {
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

        public void OnCombo()
        {
            if (_menu.Item("com.itwitch.combo.useEKillable").GetValue<bool>() && _spells[SpellSlot.E].IsReady())
            {
                var killableTarget =
                    HeroManager.Enemies.FirstOrDefault(
                        x =>
                            x.IsValidTarget(_spells[SpellSlot.E].Range) && _spells[SpellSlot.E].IsInRange(x)
                            && IsKillable(x));
                if (killableTarget != null)
                {
                    _spells[SpellSlot.E].Cast();
                }
            }
            if (_menu.Item("com.itwitch.combo.useW").GetValue<bool>() && _spells[SpellSlot.W].IsReady())
            {
                var wTarget = TargetSelector.GetTarget(_spells[SpellSlot.W].Range, TargetSelector.DamageType.Physical);
                if (wTarget.IsValidTarget(_spells[SpellSlot.W].Range))
                {
                    var prediction = _spells[SpellSlot.W].GetPrediction(wTarget);
                    if (prediction.Hitchance >= HitChance.High)
                    {
                        _spells[SpellSlot.W].Cast(prediction.CastPosition);
                    }
                }
            }
        }


        public void OnHarass()
        {
            if (_menu.Item("com.itwitch.harass.useE").GetValue<bool>() && _spells[SpellSlot.E].IsReady())
            {
                var target =
                    HeroManager.Enemies.FirstOrDefault(
                        x =>
                            x.IsValidTarget(_spells[SpellSlot.E].Range) && _spells[SpellSlot.E].IsInRange(x) &&
                            x.GetBuffCount("twitchdeadlyvenom") >=
                            _menu.Item("com.itwitch.harass.eStacks").GetValue<Slider>().Value);
                if (target != null)
                {
                    _spells[SpellSlot.E].Cast();
                }
            }
            if (_menu.Item("com.itwitch.harass.useW").GetValue<bool>() && _spells[SpellSlot.W].IsReady())
            {
                var wTarget = TargetSelector.GetTarget(_spells[SpellSlot.W].Range, TargetSelector.DamageType.Physical);
                if (wTarget.IsValidTarget(_spells[SpellSlot.W].Range))
                {
                    var prediction = _spells[SpellSlot.W].GetPrediction(wTarget);
                    if (prediction.Hitchance >= HitChance.High)
                    {
                        _spells[SpellSlot.W].Cast(prediction.CastPosition);
                    }
                }
            }
        }

        public float GetActualDamage(Obj_AI_Base target)
        {
            if (target.HasBuff("twitchdeadlyvenom"))
                return 0f;

            var baseDamage = _spells[SpellSlot.E].GetDamage(target);

            // With exhaust players damage is reduced by 40%
            if (ObjectManager.Player.HasBuff("summonerexhaust"))
            {
                return baseDamage*0.6f;
            }

            // Alistars ultimate reduces damage dealt by 70%
            if (target.HasBuff("FerociousHowl"))
            {
                return baseDamage*0.3f;
            }

            // Damage to dragon is reduced by 7% * (stacks)
            if (target.Name.Contains("Dragon") && ObjectManager.Player.HasBuff("s5test_dragonslayerbuff"))
            {
                return baseDamage*(1 - 0.7f*ObjectManager.Player.GetBuffCount("s5test_dragonslayerbuff"));
            }

            // Damage to baron is reduced by 50% if the player has the 'barontarget'
            if (target.Name.Contains("Baron") && ObjectManager.Player.HasBuff("barontarget"))
            {
                return baseDamage*0.5f;
            }

            return baseDamage;
        }

        public bool IsKillable(Obj_AI_Base target)
        {
            return GetActualDamage(target) >= target.Health + target.AllShield && !HasUndyingBuff(target) &&
                   !target.HasBuffOfType(BuffType.SpellShield);
        }

        public static bool HasUndyingBuff(Obj_AI_Base target1)
        {
            var target = target1 as Obj_AI_Hero;

            if (target == null) return false;
            // Tryndamere R
            if (target.ChampionName == "Tryndamere"
                && target.Buffs.Any(
                    b => b.Caster.NetworkId == target.NetworkId && b.IsValid && b.DisplayName == "Undying Rage"))
            {
                return true;
            }

            // Zilean R
            if (target.Buffs.Any(b => b.IsValid && b.DisplayName == "Chrono Shift"))
            {
                return true;
            }

            // Kayle R
            if (target.Buffs.Any(b => b.IsValid && b.DisplayName == "JudicatorIntervention"))
            {
                return true;
            }

            //TODO poppy

            return false;
        }
    }
}