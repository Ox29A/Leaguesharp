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
        public static float GetHealthWithShield(this Obj_AI_Base target)
            => target.AllShield > 0 ? target.Health + target.AllShield : target.Health + 10;

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
        ///     Checks if the given target is killable
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool IsRendKillable(this Obj_AI_Base target)
        {
            var champion = target as Obj_AI_Hero;
            if (champion != null &&
                (champion.HasUndyingBuff() || champion.Health < 1 || champion.HasBuffOfType(BuffType.SpellShield)))
                return false;

            var baseDamage = SpellManager.Spell[SpellSlot.E].GetDamage(target);

            //Exory Is Bae
            if (champion != null && champion.HasBuff("meditate"))
            {
                baseDamage *= (0.5f - 0.05f * champion.Spellbook.GetSpell(SpellSlot.W).Level);
            }

            if (target.Name.Contains("Baron") && ObjectManager.Player.HasBuff("barontarget"))
            {
                baseDamage *= 0.5f;
            }
            if (ObjectManager.Player.HasBuff("SummonerExhaustSlow"))
            {
                baseDamage *= 0.55f;
            }
            if (target.Name.Contains("Dragon") && ObjectManager.Player.HasBuff("s5test_dragonslayerbuff"))
            {
                baseDamage *= (1f - (0.07f*ObjectManager.Player.GetBuffCount("s5test_dragonslayerbuff")));
            }


            return baseDamage > target.GetHealthWithShield();
        }

        public static float GetRendDamage(Obj_AI_Base target)
        {
            return SpellManager.Spell[SpellSlot.E].GetDamage(target);
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

            if (target.HasBuff("kindredrnodeathbuff"))
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
            return IsRendKillable(target);
        }


        /*public static bool IsRendKillable(this Obj_AI_Hero target)
        {
            return IsRendKillable((Obj_AI_Base) target) >= GetHealthWithShield(target) && !HasUndyingBuff(target) && !target.HasBuffOfType(BuffType.SpellShield);
        }*/

        #endregion
    }
}