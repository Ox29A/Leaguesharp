using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

using SharpDX;

namespace iKalistaReborn.Utils
{
    using System.Drawing;

    internal class CustomDamageIndicator
    {
        private const int BarWidth = 104;

        private const int LineThickness = 9;

        private static Utility.HpBarDamageIndicator.DamageToUnitDelegate _damageToUnit;

        private static readonly Vector2 BarOffset = new Vector2(10, 25);

        private static Color drawingColor;

        public static Color DrawingColor
        {
            get
            {
                return drawingColor;
            }

            set
            {
                drawingColor = System.Drawing.Color.FromArgb(170, value);
            }
        }

        public static bool Enabled { get; set; }

        public static bool EnabledJ { get; set; }

        public static void Initialize(Utility.HpBarDamageIndicator.DamageToUnitDelegate damageToUnit)
        {
            // Apply needed field delegate for damage calculation
            _damageToUnit = damageToUnit;
            DrawingColor = System.Drawing.Color.Green;
            Enabled = true;

            // Register event handlers
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Enabled)
            {
                foreach (var unit in ObjectManager.Get<Obj_AI_Hero>().Where(u => u.IsValidTarget() && u.IsHPBarRendered)
                    )
                {
                    // Get damage to unit
                    var damage = _damageToUnit(unit);

                    // Continue on 0 damage
                    if (damage <= 0) continue;

                    // Get remaining HP after damage applied in percent and the current percent of health
                    var damagePercentage = ((unit.Health - damage) > 0 ? (unit.Health - damage) : 0) / unit.MaxHealth;
                    var currentHealthPercentage = unit.Health / unit.MaxHealth;

                    // Calculate start and end point of the bar indicator
                    var startPoint = new Vector2(
                        (int)(unit.HPBarPosition.X + BarOffset.X + damagePercentage * BarWidth), 
                        (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);
                    var endPoint =
                        new Vector2(
                            (int)(unit.HPBarPosition.X + BarOffset.X + currentHealthPercentage * BarWidth) + 1, 
                            (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);

                    // Draw the line
                    Drawing.DrawLine(startPoint, endPoint, LineThickness, DrawingColor);
                }
            }

            if (EnabledJ)
            {
                foreach (var unit in
                    GameObjects.Jungle.Where(
                        x =>
                        ObjectManager.Player.Distance(x) <= SpellManager.Spell[SpellSlot.E].Range && x.IsValidTarget()
                        && x.IsHPBarRendered && x.HasRendBuff()))
                {
                    Render.Circle.DrawCircle(
                        unit.Position, 
                        500f, 
                        unit.IsMobKillable() ? System.Drawing.Color.GreenYellow : System.Drawing.Color.Red);
                }
            }
        }
    }
}