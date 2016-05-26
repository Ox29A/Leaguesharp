using System;
using System.Drawing;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

namespace iKalistaReborn.Utils
{
    internal class CustomDamageIndicator
    {
        private static int height;

        private static int width;

        private static int xOffset;

        private static int yOffset;

        public static Color EnemyColor = Kalista.Menu.Item("com.ikalista.drawing.eDamage").GetValue<Circle>().Color;

        public static Color JungleColor = Kalista.Menu.Item("com.ikalista.drawing.eDamageJ").GetValue<Circle>().Color;

        private static DamageToUnitDelegate damageToUnit;

        public delegate float DamageToUnitDelegate(Obj_AI_Base minion);

        public static DamageToUnitDelegate DamageToUnit
        {
            get
            {
                return damageToUnit;
            }

            set
            {
                if (damageToUnit == null)
                {
                    Drawing.OnEndScene += OnEndScene;
                }

                damageToUnit = value;
            }
        }

        private static void OnEndScene(EventArgs args)
        {
            if (Kalista.Menu.Item("com.ikalista.drawing.eDamage").GetValue<Circle>().Active)
            {
                foreach (
                    var hero in
                        GameObjects.EnemyHeroes.Where(x => x.IsValidTarget() && x.IsHPBarRendered && x.HasRendBuff()))
                {
                    height = 9;
                    width = 104;
                    xOffset = hero.ChampionName == "Jhin" ? -9 : 2;
                    yOffset = hero.ChampionName == "Jhin" ? -5 : 9;

                    DrawLine(hero);
                }
            }

            if (Kalista.Menu.Item("com.ikalista.drawing.eDamageJ").GetValue<Circle>().Active)
            {
                foreach (
                    var unit in
                        GameObjects.Jungle.Where(
                            x =>
                            ObjectManager.Player.Distance(x) <= SpellManager.Spell[SpellSlot.E].Range
                            && x.IsValidTarget() && x.IsHPBarRendered && x.HasRendBuff()))
                {
                    Render.Circle.DrawCircle(unit.Position, 500f, unit.IsMobKillable() ? Color.GreenYellow : Color.Red);
                }
            }
        }

        private static void DrawLine(Obj_AI_Base unit)
        {
            var damage = damageToUnit(unit);
            if (damage <= 0) return;

            var barPos = unit.HPBarPosition;

            // Get remaining HP after damage applied in percent and the current percent of health
            var percentHealthAfterDamage = Math.Max(0, unit.Health - damage)
                                           / (unit.MaxHealth + unit.AllShield + unit.PhysicalShield + unit.MagicalShield);
            var currentHealthPercentage = unit.Health
                                          / (unit.MaxHealth + unit.AllShield + unit.PhysicalShield + unit.MagicalShield);

            // Calculate start and end point of the bar indicator
            var startPoint = barPos.X + xOffset + (percentHealthAfterDamage * width);
            var endPoint = barPos.X + xOffset + (currentHealthPercentage * width);
            var yPos = barPos.Y + yOffset;

            // Draw the line
            Drawing.DrawLine(startPoint, yPos, endPoint, yPos, height, unit is Obj_AI_Hero ? EnemyColor : JungleColor);
        }
    }
}