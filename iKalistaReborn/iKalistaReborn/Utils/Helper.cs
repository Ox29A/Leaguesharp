using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Collision = LeagueSharp.Common.Collision;

namespace iKalistaReborn.Utils
{
    /// <summary>
    ///     The Helper class
    /// </summary>
    internal static class Helper
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Gets the list of minions currently between the source and target
        /// </summary>
        /// <param name="source">
        ///     The Source
        /// </param>
        /// <param name="targetPosition">
        ///     The Target Position
        /// </param>
        /// <returns>
        ///     The <see cref="List" />.
        /// </returns>
        public static List<Obj_AI_Base> GetCollisionMinions(Obj_AI_Hero source, Vector3 targetPosition)
        {
            var input = new PredictionInput
            {
                Unit = source,
                Radius = SpellManager.Spell[SpellSlot.Q].Width,
                Delay = SpellManager.Spell[SpellSlot.Q].Delay,
                Speed = SpellManager.Spell[SpellSlot.Q].Speed,
                CollisionObjects = new[] {CollisionableObjects.Minions}
            };

            return
                Collision.GetCollision(new List<Vector3> {targetPosition}, input)
                    .OrderBy(x => x.Distance(source))
                    .ToList();
        }

        /// <summary>
        ///     Gets the targets current health including shield damage
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        public static float GetHealthWithShield(this Obj_AI_Base target) => target.AllShield > 0 ? target.Health + target.AllShield : target.Health + 10;

        /// <summary>
        ///     Gets the rend buff
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="BuffInstance" />.
        /// </returns>
        public static BuffInstance GetRendBuff(this Obj_AI_Base target)
        {
            return
                target.Buffs.Find(
                    b => b.Caster.IsMe && b.IsValid && b.DisplayName.ToLowerInvariant() == "kalistaexpungemarker");
        }

        /// <summary>
        ///     Gets the current <see cref="BuffInstance" /> Count of Expunge
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
        public static int GetRendBuffCount(this Obj_AI_Base target)
        {
            return target.Buffs.Count(x => x.Name == "kalistaexpungemarker");
        }

        /// <summary>
        ///     Gets the Rend Damage for each target
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        public static float GetRendDamage(Obj_AI_Base target)
        {
            // If that target doesn't have a rend stack then calculating this is pointless
            if (!target.HasRendBuff() || target.Health < 1)
            {
                return 0f;
            }

            // The base damage of E
            var baseDamage = SpellManager.Spell[SpellSlot.E].GetDamage(target);

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

            // Master YI reduces the damage taken by 50/55/60/65/70 based on his W Level.
            if (target.HasBuff("meditate"))
            {
                return baseDamage*(0.5f - 0.05f * target.Spellbook.GetSpell(SpellSlot.W).Level);
            }

            // Damage to dragon is reduced by 7% * (stacks)
            if (target.Name.Contains("Dragon") && ObjectManager.Player.HasBuff("s5test_dragonslayerbuff"))
            {
                return baseDamage*(1f - 0.075f*ObjectManager.Player.GetBuffCount("s5test_dragonslayerbuff"));
            }

            // Damage to baron is reduced by 50% if the player has the 'barontarget'
            if (target.Name.Contains("Baron") && ObjectManager.Player.HasBuff("barontarget"))
            {
                return baseDamage*0.5f;
            }

            return baseDamage;
        }

        /// <summary>
        ///     Checks if a target has the Expunge <see cref="BuffInstance" />
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool HasRendBuff(this Obj_AI_Base target)
        {
            return target?.GetRendBuff() != null;
        }

        /// <summary>
        ///     Checks if the given target has an invulnerable buff
        /// </summary>
        /// <param name="target1">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool HasUndyingBuff(this Obj_AI_Base target1)
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

            if (target.Buffs.Any(b => b.IsValid && b.DisplayName == "kindredrnodeathbuff"))
            {
                return true;
            }

            //TODO poppy

            return false;
        }

        /// <summary>
        ///     TODO The is mob killable.
        /// </summary>
        /// <param name="target">
        ///     TODO The target.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool IsMobKillable(this Obj_AI_Base target)
        {
            return GetRendDamage(target) >= target.Health;
        }

        /// <summary>
        ///     Checks if the given target is killable
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool IsRendKillable(this Obj_AI_Hero target)
        {
            return GetRendDamage(target) >= GetHealthWithShield(target) && !HasUndyingBuff(target) && !target.HasBuffOfType(BuffType.SpellShield);
        }

        #endregion
    }
}
