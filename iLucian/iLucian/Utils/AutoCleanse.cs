namespace iLucian.Utils
{
    using System;
    using System.Collections.Generic;

    using DZLib.MenuExtensions;

    using LeagueSharp;
    using LeagueSharp.Common;

    class AutoCleanse
    {
        #region Static Fields

        private static readonly List<DangerousSpell> DangerousSpells = new List<DangerousSpell>
                                                                           {
                                                                               new DangerousSpell
                                                                                   {
                                                                                       ChampName = "Warwick", 
                                                                                       IsEnabled = true, 
                                                                                       SpellBuff = "InfiniteDuress", 
                                                                                       SpellName = "Warwick R", 
                                                                                       RealName = "warwickR", 
                                                                                       OnlyKill = false, 
                                                                                       Slot = SpellSlot.R, Delay = 100f
                                                                                   }, 
                                                                               new DangerousSpell
                                                                                   {
                                                                                       ChampName = "Rammus", 
                                                                                       IsEnabled = true, 
                                                                                       SpellBuff = "PuncturingTaunt", 
                                                                                       SpellName = "Rammus E", 
                                                                                       RealName = "rammusE", 
                                                                                       OnlyKill = false, 
                                                                                       Slot = SpellSlot.E, Delay = 100f
                                                                                   }, 
                                                                               new DangerousSpell
                                                                                   {
                                                                                       ChampName = "Amumu", 
                                                                                       IsEnabled = true, 
                                                                                       SpellBuff = "CurseoftheSadMummy", 
                                                                                       SpellName = "Amumu R", 
                                                                                       RealName = "Amumu R", 
                                                                                       OnlyKill = false, 
                                                                                       Slot = SpellSlot.R, Delay = 250f
                                                                                   }, 
                                                                               /** Danger Level 4 Spells*/
                                                                               new DangerousSpell
                                                                                   {
                                                                                       ChampName = "Skarner", 
                                                                                       IsEnabled = true, 
                                                                                       SpellBuff = "SkarnerImpale", 
                                                                                       SpellName = "Skaner R", 
                                                                                       RealName = "skarnerR", 
                                                                                       OnlyKill = false, 
                                                                                       Slot = SpellSlot.R, Delay = 100f
                                                                                   }, 
                                                                               new DangerousSpell
                                                                                   {
                                                                                       ChampName = "Fizz", 
                                                                                       IsEnabled = true, 
                                                                                       SpellBuff = "FizzMarinerDoom", 
                                                                                       SpellName = "Fizz R", 
                                                                                       RealName = "FizzR", 
                                                                                       OnlyKill = false, 
                                                                                       Slot = SpellSlot.R, Delay = 100f
                                                                                   }, 
                                                                               new DangerousSpell
                                                                                   {
                                                                                       ChampName = "Galio", 
                                                                                       IsEnabled = true, 
                                                                                       SpellBuff = "GalioIdolOfDurand", 
                                                                                       SpellName = "Galio R", 
                                                                                       RealName = "GalioR", 
                                                                                       OnlyKill = false, 
                                                                                       Slot = SpellSlot.R, Delay = 100f
                                                                                   }, 
                                                                               new DangerousSpell
                                                                                   {
                                                                                       ChampName = "Malzahar", 
                                                                                       IsEnabled = true, 
                                                                                       SpellBuff = "AlZaharNetherGrasp", 
                                                                                       SpellName = "Malz R", 
                                                                                       RealName = "MalzaharR", 
                                                                                       OnlyKill = false, 
                                                                                       Slot = SpellSlot.R, Delay = 200f
                                                                                   }, 
                                                                               new DangerousSpell
                                                                                   {
                                                                                       ChampName = "Vladimir", 
                                                                                       IsEnabled = false, 
                                                                                       SpellBuff = "VladimirHemoplague", 
                                                                                       SpellName = "Vlad R", 
                                                                                       RealName = "VladimirR", 
                                                                                       OnlyKill = true, Slot = SpellSlot.R, 
                                                                                       Delay = 700f
                                                                                   }, 
                                                                               new DangerousSpell
                                                                                   {
                                                                                       ChampName = "Mordekaiser", 
                                                                                       IsEnabled = true, 
                                                                                       SpellBuff =
                                                                                           "MordekaiserChildrenOfTheGrave", 
                                                                                       SpellName = "Morde R", 
                                                                                       OnlyKill = true, Slot = SpellSlot.R, 
                                                                                       Delay = 800f
                                                                                   }
                                                                           };

        #endregion

        #region Public Methods and Operators

        public static void Initialize()
        {
            var autocleanseMenu = new Menu(":: iLucian - Auto Cleanse", "com.ilucian.autoCleanse");
            {
                autocleanseMenu.AddBool("com.ilucian.autoCleanse.enabled", "Enabled", true);
                autocleanseMenu.AddBool("com.ilucian.autoCleanse.cleanseDangerous", "Only Cleanse Dangerous", true);
                Variables.Menu.AddSubMenu(autocleanseMenu);
            }

            Game.OnUpdate += OnUpdate;
        }

        #endregion

        #region Methods

        private static void OnUpdate(EventArgs args)
        {
            if (!Variables.Menu.Item("com.ilucian.autoCleanse.enabled").GetValue<bool>()) return;

            var hasItem = Items.HasItem(3140) || Items.HasItem(3139);
            var itemReady = hasItem && (Items.CanUseItem(3140) || Items.CanUseItem(3139));

            if (!hasItem || !itemReady) return;
            if (!Variables.Menu.Item("com.ilucian.autoCleanse.cleanseDangerous").GetValue<bool>()) return;
            foreach (var spell in DangerousSpells)
            {
                if (ObjectManager.Player.HasBuff(spell.SpellBuff))
                {
                    Items.UseItem(3139);
                    Items.UseItem(3140);
                }
                else
                {
                    if (!ObjectManager.Player.HasBuffOfType(BuffType.Charm)
                        && !ObjectManager.Player.HasBuffOfType(BuffType.Flee)
                        && !ObjectManager.Player.HasBuffOfType(BuffType.Polymorph)
                        && !ObjectManager.Player.HasBuffOfType(BuffType.Snare)
                        && !ObjectManager.Player.HasBuffOfType(BuffType.Stun)
                        && !ObjectManager.Player.HasBuffOfType(BuffType.Suppression)
                        && !ObjectManager.Player.HasBuffOfType(BuffType.Taunt)
                        && (!ObjectManager.Player.HasBuff("AhriSeduce")
                            || ObjectManager.Player.HasBuffOfType(BuffType.SpellShield)
                            || ObjectManager.Player.HasBuffOfType(BuffType.SpellImmunity))) continue;
                    Items.UseItem(3139);
                    Items.UseItem(3140);
                }
            }
        }

        #endregion
    }

    public class DangerousSpell
    {
        #region Public Properties

        public string ChampName { get; set; }

        public float Delay { get; set; }

        public bool IsEnabled { get; set; }

        public bool OnlyKill { get; set; }

        public string RealName { get; set; }

        public SpellSlot Slot { get; set; }

        public string SpellBuff { get; set; }

        public string SpellName { get; set; }

        #endregion
    }
}