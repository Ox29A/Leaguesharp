using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZLib.Core;
using DZLib.Positioning;
using iLucian.MenuHelper;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace iLucian
{
    class Lucian
    {
        public void OnLoad()
        {
            Console.WriteLine("Loaded Lucian");
            MenuGenerator.Generate();
            LoadSpells();
            LoadEvents();
        }

        private void LoadEvents()
        {
            Game.OnUpdate += OnUpdate;
            Obj_AI_Base.OnDoCast += OnDoCast;
            DZAntigapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
        }

        private static void OnEnemyGapcloser(DZLib.Core.ActiveGapcloser gapcloser)
        {
            if (!Variables.Menu.IsEnabled("com.ilucian.misc.gapcloser"))
            {
                return;
            }
            
            if (!gapcloser.Sender.IsEnemy || !(gapcloser.End.Distance(ObjectManager.Player.ServerPosition) < 350))
                return;

            var extendedPosition = ObjectManager.Player.ServerPosition.Extend(Game.CursorPos,
                Variables.Spell[Variables.Spells.E].Range);
            if (extendedPosition.IsSafe(Variables.Spell[Variables.Spells.E].Range) &&
                extendedPosition.CountAlliesInRange(650f) > 0)
            {
                Variables.Spell[Variables.Spells.E].Cast(extendedPosition);
            }
        }

        private void OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.SData.Name != "LucianPassiveShot" && !args.SData.Name.Contains("LucianBasicAttack"))
                return;
            if (Variables.HasPassive)
                return;

            var target = TargetSelector.GetTarget(Variables.Spell[Variables.Spells.Q].Range,
                TargetSelector.DamageType.Physical);
            if (target == null || !(ObjectManager.Player.Distance(target) < ObjectManager.Player.AttackRange))
                return;
            switch (Variables.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (target.IsValidTarget(Variables.Spell[Variables.Spells.Q].Range) &&
                        Variables.Menu.Item("com.ilucian.combo.q").GetValue<bool>())
                    {
                        if (Variables.Spell[Variables.Spells.Q].IsReady() &&
                            Variables.Spell[Variables.Spells.Q].IsInRange(target))
                        {
                            Variables.Spell[Variables.Spells.Q].Cast(target);
                        }
                    }
                    if (!ObjectManager.Player.IsDashing() &&
                        Variables.Menu.Item("com.ilucian.combo.w").GetValue<bool>())
                    {
                        if (Variables.Spell[Variables.Spells.W].IsReady())
                        {
                            if (Variables.Menu.IsEnabled("com.ilucian.misc.usePrediction"))
                            {
                                var prediction = Variables.Spell[Variables.Spells.W].GetPrediction(target);
                                if (prediction.Hitchance >= HitChance.High)
                                {
                                    Variables.Spell[Variables.Spells.W].Cast(prediction.CastPosition);
                                }
                            }
                        }
                        else
                        {
                            if (target.Distance(ObjectManager.Player) < 600)
                            {
                                Variables.Spell[Variables.Spells.W].Cast(target.Position);
                            }
                        }
                    }
                    CastE(target);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (target.IsValidTarget(Variables.Spell[Variables.Spells.Q].Range) &&
                        Variables.Menu.Item("com.ilucian.harass.q").GetValue<bool>())
                    {
                        if (Variables.Spell[Variables.Spells.Q].IsReady() &&
                            Variables.Spell[Variables.Spells.Q].IsInRange(target))
                        {
                            Variables.Spell[Variables.Spells.Q].Cast(target);
                        }
                    }
                    if (!ObjectManager.Player.IsDashing() &&
                        Variables.Menu.Item("com.ilucian.harass.w").GetValue<bool>())
                    {
                        if (Variables.Spell[Variables.Spells.W].IsReady())
                        {
                            if (Variables.Menu.IsEnabled("com.ilucian.misc.usePrediction"))
                            {
                                var prediction = Variables.Spell[Variables.Spells.W].GetPrediction(target);
                                if (prediction.Hitchance >= HitChance.High)
                                {
                                    Variables.Spell[Variables.Spells.W].Cast(prediction.CastPosition);
                                }
                            }
                        }
                        else
                        {
                            if (target.Distance(ObjectManager.Player) < 600)
                            {
                                Variables.Spell[Variables.Spells.W].Cast(target.Position);
                            }
                        }
                    }
                    break;
            }
        }

        private void LoadSpells()
        {
            Variables.Spell[Variables.Spells.Q].SetTargetted(0.25f, 1400f);
            Variables.Spell[Variables.Spells.Q2].SetSkillshot(0.5f, 50, float.MaxValue, false,
                SkillshotType.SkillshotLine);
            Variables.Spell[Variables.Spells.W].SetSkillshot(0.30f, 70f, 1600f, true, SkillshotType.SkillshotLine);
            Variables.Spell[Variables.Spells.R].SetSkillshot(0.2f, 110f, 2500, true, SkillshotType.SkillshotLine);
        }

        private void OnUpdate(EventArgs args)
        {
            Variables.Spell[Variables.Spells.W].Collision =
                Variables.Menu.Item("com.ilucian.misc.usePrediction").GetValue<bool>();
            switch (Variables.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    OnCombo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    OnHarass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    OnLaneclear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    break;
                case Orbwalking.OrbwalkingMode.Freeze:
                    break;
                case Orbwalking.OrbwalkingMode.CustomMode:
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnCombo()
        {
            var target = TargetSelector.GetTarget(Variables.Spell[Variables.Spells.Q2].Range, TargetSelector.DamageType.Physical);

            if (target == null || Variables.HasPassive)
                return;
            if (Variables.Menu.IsEnabled("com.ilucian.combo.qExtended"))
            {
                CastExtendedQ();
            }
            if (target.IsValidTarget(Variables.Spell[Variables.Spells.Q].Range) && Variables.Menu.IsEnabled("com.ilucian.combo.q"))
            {
                if (Variables.Spell[Variables.Spells.Q].IsReady() && Variables.Spell[Variables.Spells.Q].IsInRange(target))
                {
                    Variables.Spell[Variables.Spells.Q].Cast(target);
                }
            }
            if (ObjectManager.Player.IsDashing() || !Variables.Menu.IsEnabled("com.ilucian.combo.w")) return;
            if (!Variables.Spell[Variables.Spells.W].IsReady()) return;

            if (Variables.Menu.IsEnabled("com.ilucian.misc.usePrediction"))
            {
                var prediction = Variables.Spell[Variables.Spells.W].GetPrediction(target);
                if (prediction.Hitchance >= HitChance.High)
                {
                    Variables.Spell[Variables.Spells.W].Cast(prediction.CastPosition);
                }
            }
            else
            {
                if (target.Distance(ObjectManager.Player) < 600)
                {
                    Variables.Spell[Variables.Spells.W].Cast(target.Position);
                }
            }
        }

        private void OnHarass()
        {
            var target = TargetSelector.GetTarget(Variables.Spell[Variables.Spells.Q].Range, TargetSelector.DamageType.Physical);

            if (target == null || Variables.HasPassive)
                return;
            if (Variables.Menu.IsEnabled("com.ilucian.harass.qExtended"))
            {
                CastExtendedQ();
            }
            if (target.IsValidTarget(Variables.Spell[Variables.Spells.Q].Range) && Variables.Menu.IsEnabled("com.ilucian.harass.q"))
            {
                if (Variables.Spell[Variables.Spells.Q].IsReady() && Variables.Spell[Variables.Spells.Q].IsInRange(target))
                {
                    Variables.Spell[Variables.Spells.Q].Cast(target);
                }
            }
            if (!ObjectManager.Player.IsDashing() && Variables.Menu.IsEnabled("com.ilucian.harass.w"))
            {
                if (Variables.Spell[Variables.Spells.W].IsReady())
                {
                    if (Variables.Menu.IsEnabled("com.ilucian.misc.usePrediction"))
                    {
                        var prediction = Variables.Spell[Variables.Spells.W].GetPrediction(target);
                        if (prediction.Hitchance >= HitChance.High)
                        {
                            Variables.Spell[Variables.Spells.W].Cast(prediction.CastPosition);
                        }
                    }
                    else
                    {
                        if (target.Distance(ObjectManager.Player) < 600)
                        {
                            Variables.Spell[Variables.Spells.W].Cast(target.Position);
                        }
                    }
                }
            }
        }

        private void OnLaneclear()
        {
            if (Variables.Menu.IsEnabled("com.ilucian.laneclear.q"))
            {
                var minions = MinionManager.GetMinions(Variables.Spell[Variables.Spells.Q].Range);
                var bestLocation = Variables.Spell[Variables.Spells.Q].GetCircularFarmLocation(minions, 60);

                if (bestLocation.MinionsHit < Variables.Menu.Item("com.ilucian.laneclear.qMinions").GetValue<Slider>().Value)
                    return;
                var adjacentMinions = minions.Where(m => m.Distance(bestLocation.Position) <= 45).ToList();
                if (!adjacentMinions.Any())
                {
                    return;
                }

                var firstMinion = adjacentMinions.OrderBy(m => m.Distance(bestLocation.Position)).First();

                if (!firstMinion.IsValidTarget(Variables.Spell[Variables.Spells.Q].Range))
                    return;
                if (!Variables.HasPassive && Orbwalking.InAutoAttackRange(firstMinion))
                {
                    Variables.Spell[Variables.Spells.Q].Cast(firstMinion);
                }
            }
        }

        private void CastE(Obj_AI_Base target)
        {
            if (!Variables.Spell[Variables.Spells.E].IsReady() || !Variables.Menu.IsEnabled("com.ilucian.combo.e") || target == null)
            {
                return;
            }

            if (HeroManager.Player.HasBuff("AwesomeBuff"))
            {
                var extendedPosition = ObjectManager.Player.ServerPosition.Extend(Game.CursorPos, Variables.Spell[Variables.Spells.E].Range);
                if (extendedPosition.IsSafe(Variables.Spell[Variables.Spells.E].Range))
                {
                    Variables.Spell[Variables.Spells.E].Cast(Game.CursorPos);
                }
                return;
            }

            switch (Variables.Menu.Item("com.ilucian.combo.eMode").GetValue<StringList>().SelectedIndex)
            {
                case 0: // kite
                    var hypotheticalPosition = ObjectManager.Player.ServerPosition.Extend(Game.CursorPos, Variables.Spell[Variables.Spells.E].Range);
                    if (ObjectManager.Player.HealthPercent <= 70 && target.HealthPercent >= ObjectManager.Player.HealthPercent)
                    {
                        if (ObjectManager.Player.Position.Distance(ObjectManager.Player.ServerPosition) >= 35 && target.Distance(ObjectManager.Player.ServerPosition) < target.Distance(ObjectManager.Player.Position) && hypotheticalPosition.IsSafe(Variables.Spell[Variables.Spells.E].Range))
                        {
                            Variables.Spell[Variables.Spells.E].Cast(hypotheticalPosition);
                        }
                    }

                    if (hypotheticalPosition.IsSafe(Variables.Spell[Variables.Spells.E].Range) && hypotheticalPosition.Distance(target.ServerPosition) <= Orbwalking.GetRealAutoAttackRange(null) && (hypotheticalPosition.Distance(target.ServerPosition) > 400) && !Variables.HasPassive)
                    {
                        Variables.Spell[Variables.Spells.E].Cast(hypotheticalPosition);
                    }
                    break;

                case 1: // side
                    Variables.Spell[Variables.Spells.E].Cast(Deviation(ObjectManager.Player.Position.To2D(), target.Position.To2D(), 65).To3D());
                    break;

                case 2: //Cursor
                    if (Game.CursorPos.IsSafe(475))
                    {
                        Variables.Spell[Variables.Spells.E].Cast(ObjectManager.Player.Position.Extend(Game.CursorPos, 400));
                    }
                    break;

                case 3: // Enemy
                    Variables.Spell[Variables.Spells.E].Cast(ObjectManager.Player.Position.Extend(target.Position, 400));
                    break;
            }
        }

        private void CastExtendedQ()
        {
            if (!Variables.Spell[Variables.Spells.Q].IsReady())
            {
                return;
            }
            var target = TargetSelector.GetTarget(Variables.Spell[Variables.Spells.Q2].Range, TargetSelector.DamageType.Physical);
            var minionsAround = MinionManager.GetMinions(Variables.Spell[Variables.Spells.Q].Range);
            foreach (var minion in minionsAround)
            {
                var qRetangle = new Geometry.Polygon.Rectangle(ObjectManager.Player.Position, ObjectManager.Player.Position.Extend(minion.Position, Variables.Spell[Variables.Spells.Q2].Range), Variables.Spell[Variables.Spells.Q2].Width);
                var prediction = Variables.Spell[Variables.Spells.Q2].GetPrediction(target);
                if (!qRetangle.IsOutside(prediction.UnitPosition.To2D()) && prediction.Hitchance >= HitChance.High)
                {
                    Variables.Spell[Variables.Spells.Q].Cast(minion);
                }
            }
        }

        /// <summary>
        ///     Credits to Myo, stolen from him, ily :^)
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public Vector2 Deviation(Vector2 point1, Vector2 point2, double angle)
        {
            angle *= Math.PI/180.0;
            var temp = Vector2.Subtract(point2, point1);
            var result = new Vector2(0)
            {
                X = (float) (temp.X*Math.Cos(angle) - temp.Y*Math.Sin(angle))/4, Y = (float) (temp.X*Math.Sin(angle) + temp.Y*Math.Cos(angle))/4
            };
            result = Vector2.Add(result, point1);
            return result;
        }
    }
}