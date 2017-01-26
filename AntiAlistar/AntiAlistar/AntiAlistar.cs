using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace AntiAlistar
{
    internal class AntiAlistar
    {
        #region Methods

        private static void OnGapcloser(ActiveGapcloser gapcloser)
        {
            if (ObjectManager.Player.Position.Distance(gapcloser.End) > 365f) return;

            if (!gapcloser.Sender.IsEnemy || gapcloser.SkillType != GapcloserType.Targeted ||
                gapcloser.Sender.ChampionName != "Alistar")
                return;

            if (FlashSlot.IsReady()
                && ObjectManager.Player.GetEnemiesInRange(1500f).Count
                >= Menu.Item("com.antiali.flashAmount").GetValue<Slider>().Value
                && ObjectManager.Player.HealthPercent
                < Menu.Item("com.antiali.flashPercent").GetValue<Slider>().Value)
            {
                if (SupportedChampions.Contains(ObjectManager.Player.ChampionName) && ChampionSpell.IsReady())
                    return;

                ObjectManager.Player.Spellbook.CastSpell(
                    FlashSlot,
                    GetSelectedPosition(gapcloser.Sender.Position, 450));
            }

            if (!SupportedChampions.Contains(ObjectManager.Player.ChampionName)) return;
            if (!Menu.Item("com.antiali.useSpell").GetValue<bool>() || !ChampionSpell.IsReady()) return;

            if (IsTargeted())
            {
                ChampionSpell.CastOnUnit(gapcloser.Sender);
            }
            else
            {
                var position = GetSelectedPosition(gapcloser.Sender.Position, ChampionSpell.Range);
                ChampionSpell.Cast(position);
            }
        }

        #endregion

        #region Static Fields

        public static Spell ChampionSpell;

        public static SpellSlot FlashSlot;

        public static Menu Menu;

        public static string[] SupportedChampions = {"Vayne", "Ezreal", "Lucian", "Graves"};

        #endregion

        #region Public Methods and Operators

        /**
         *  1. Customizable hp percentage for flashing when no dashes available. - done
            2. Flash if X enemies close to alistar. - done
            3. Drawing that shows if Alistar is in range for combo - todo
            4. Drawing that shows if Alistar is in range for flash Q/flash W - todo 
            5. Anti alistar support for all possible adc champs (or more) dashes  - todo more
            6. Where to flash (backwards, teammates, etc) - half done
            Q = 365, W = 650f - ranges
         */

        public static Spell GetChampionSpell()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Vayne":
                    return new Spell(SpellSlot.E, 550f);
                case "Lucian":
                    return new Spell(SpellSlot.E, 475f);
                case "Ezreal":
                    return new Spell(SpellSlot.E, 475f);
                case "Graves":
                    return new Spell(SpellSlot.E, 425f);
            }

            return null;
        }

        public static Vector3 GetSelectedPosition(Vector3 pos, float range)
        {
            switch (Menu.Item("com.antiali.flashPosition").GetValue<StringList>().SelectedIndex)
            {
                case 0: // backwards
                    return ObjectManager.Player.ServerPosition.Extend(pos, -range);
                case 1: // teammates
                    var teammate = ObjectManager.Player.GetAlliesInRange(1500f).FirstOrDefault();
                    return ObjectManager.Player.Position.Extend(teammate?.Position ?? Game.CursorPos, range);
                case 2: // turret
                    var closestTurret =
                        ObjectManager.Get<Obj_AI_Turret>()
                            .FirstOrDefault(x => x.IsAlly && x.Health > 1 && x.Distance(ObjectManager.Player) < 1500f);
                    return ObjectManager.Player.Position.Extend(closestTurret?.Position ?? Game.CursorPos, range);
            }

            return ObjectManager.Player.ServerPosition.Extend(pos, -range);
        }

        public static bool IsTargeted()
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Vayne":
                    return true;
            }

            return false;
        }

        public static void DrawTextOnScreen(Vector3 location, string message, Color colour)
        {
            var world = Drawing.WorldToScreen(location);
            Drawing.DrawText(world[0] - message.Length * 5, world[1] - 200, colour, message);
        }

        private static void AlistarDrawing(EventArgs args)
        {
            const float qRange = 365f;
            const float wRange = 650f;
            const float flashQRange = 450 + qRange;
            const float flashWRange = 450 + wRange;
            if (!Menu.Item("com.antiali.drawRange").GetValue<bool>()) return;
            var alistar = HeroManager.Enemies.FirstOrDefault(x => x.ChampionName == "Alistar");
            if (alistar == null) return;

            var position = new Vector3(
                ObjectManager.Player.Position.X,
                ObjectManager.Player.Position.Y - 30,
                ObjectManager.Player.Position.Z);

            if (ObjectManager.Player.Distance(alistar) <= wRange)
                DrawTextOnScreen(position, "Alistar can W", Color.Red);
            else if (ObjectManager.Player.Distance(alistar) <= flashWRange)
                DrawTextOnScreen(position, "Alistar Can Flash W", Color.Red);
            else if (ObjectManager.Player.Distance(alistar) <= flashQRange)
                DrawTextOnScreen(position, "Alistar Can Flash Q", Color.Red);
            else
                DrawTextOnScreen(position, "Safe!", Color.GreenYellow);
        }

        public static void OnLoad(EventArgs args)
        {
            FlashSlot = ObjectManager.Player.GetSpellSlot("summonerflash");

            if (SupportedChampions.Contains(ObjectManager.Player.ChampionName))
                ChampionSpell = GetChampionSpell();

            // Menu
            Menu = new Menu("Anti Alistar", "com.antiali", true);
            Menu.AddItem(
                new MenuItem("com.antiali.useFlash", "Use Flash").SetValue(false)
                    .SetTooltip("Will only use flash if spell is on cd"));
            Menu.AddItem(new MenuItem("com.antiali.flashPercent", "Health For Flash").SetValue(new Slider(20)));
            Menu.AddItem(new MenuItem("com.antiali.flashAmount", "Flash if x enemies").SetValue(new Slider(2, 1, 5)));
            Menu.AddItem(new MenuItem("com.antiali.useSpell", "Use Spell").SetValue(false));
            Menu.AddItem(
                new MenuItem("info", "Flash Position Info").SetTooltip(
                    "WWill dash / flash to pos if its not founud it will cast to cursor pos."));
            Menu.AddItem(
                new MenuItem("com.antiali.flashPosition", "Flash Positionn").SetValue(
                    new StringList(new[] {"Backwards", "Teammates", "Towards Turret"})));
            Menu.AddItem(new MenuItem("com.antiali.drawRange", "Draw Text for ali range").SetValue(false));
            Menu.AddToMainMenu();

            // Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            AntiGapcloser.OnEnemyGapcloser += OnGapcloser;
            Drawing.OnDraw += AlistarDrawing;
        }

        #endregion
    }
}