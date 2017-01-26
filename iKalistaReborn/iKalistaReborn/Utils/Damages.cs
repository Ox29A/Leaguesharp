using LeagueSharp;
using LeagueSharp.Common;

namespace iKalistaReborn.Utils
{
    /// <summary>
    ///     TODO The damages.
    /// </summary>
    internal static class Damages
    {
        private const string BuffName = "kalistaexpungemarker";
        public static Spell RendSpell = SpellManager.Spell[SpellSlot.E];

        private static float BaseRendDamage => new[] {20f, 30f, 40f, 50f, 60f}[RendSpell.Level - 1];

        private static float AdditionalRendDamage => new[] {0.6f, 0.6f, 0.6f, 0.6f, 0.6f}[RendSpell.Level - 1];

        private static float SpearDamagePerStack => new[] {5, 9, 14, 20, 27}[RendSpell.Level - 1];

        private static float AdditionalSpearDamage => new[] {0.15f, 0.18f, 0.21f, 0.24f, 0.27f}[RendSpell.Level - 1];

        public static bool IsRendKillable(this Obj_AI_Base @base)
        {
            if (@base == null || !@base.IsValidTarget() || !@base.HasRendBuff())
                return false;

            var target = @base as Obj_AI_Hero;
            var baseDamage = Kalista.Menu.Item("com.ikalista.misc.damage").GetValue<StringList>().SelectedIndex == 0
                ? SpellManager.Spell[SpellSlot.E].GetDamage(target)
                : GetCalculatedRendDamage(target);

            if (target == null) return baseDamage >= @base.GetHealthWithShield();

            if (target.HasUndyingBuff() || target.HasBuffOfType(BuffType.SpellShield) ||
                target.HasBuffOfType(BuffType.SpellImmunity))
                return false;

            return baseDamage >= @base.GetHealthWithShield();
        }

        public static float GetQDamage(Obj_AI_Base target)
        {
            return new[] {10f, 70f, 130f, 190f, 250f}[SpellManager.Spell[SpellSlot.Q].Level - 1] +
                   ObjectManager.Player.BaseAttackDamage +
                   ObjectManager.Player.FlatPhysicalDamageMod;
        }

        public static float GetRawRendDamage(Obj_AI_Base target)
        {
            var rendBuff = target?.GetRendBuff();

            if (rendBuff == null) return 0;

            var stacks = rendBuff.Count;
            return BaseRendDamage + stacks * SpearDamagePerStack +
                   ObjectManager.Player.TotalAttackDamage * (AdditionalRendDamage + stacks * AdditionalSpearDamage);
        }

        public static float GetCalculatedRendDamage(this Obj_AI_Base hero)
        {
            return (float) ObjectManager.Player.CalcDamage(hero, Damage.DamageType.Physical, GetRawRendDamage(hero));
        }
    }
}