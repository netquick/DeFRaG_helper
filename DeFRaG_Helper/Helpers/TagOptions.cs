using System.Collections.Generic;

namespace DeFRaG_Helper.Helpers
{
    public static class TagOptions
    {
        public static readonly Dictionary<string, (string path, string color)> Weapons = new Dictionary<string, (string, string)>
        {
            {"Rocket Launcher", ("Icons/Weapons/iconw_rocket.svg", "White")},
            {"Plasmagun", ("Icons/Weapons/iconw_plasma.svg", "White")},
            {"Railgun", ("Icons/Weapons/iconw_railgun.svg", "White")},
            {"Shotgun", ("Icons/Weapons/iconw_shotgun.svg", "White")},
            {"Lightning Gun", ("Icons/Weapons/iconw_lightning.svg", "White")},
            {"Big fucking gun", ("Icons/Weapons/icon_bfg.svg", "White")},
            {"Gauntlet", ("Icons/Weapons/iconw_gauntlet.svg", "White")},
            {"Grappling hook", ("Icons/Weapons/iconw_grapple.svg", "White")},
            {"Grenade Launcher", ("Icons/Weapons/iconw_grenade.svg", "White")},
            {"Machinegun", ("Icons/Weapons/icon_mg.svg", "White")},
            {"Proximity Mine Launcher (Team Arena)", ("Icons/Weapons/proxmine.svg", "White")},
            {"Chaingun (Team Arena)", ("Icons/Weapons/chaingun.svg", "White")},
            {"Nailgun (Team Arena)", ("Icons/Weapons/nailgun.svg", "White")}
        };

        public static readonly Dictionary<string, (string path, string color)> Items = new Dictionary<string, (string, string)>
        {
            {"Body Armor (Red Armor)", ("Icons/Items/iconr_red.svg", "White")},
            {"Combat Armor (Yellow Armor)", ("Icons/Items/iconr_yellow.svg", "White")},
            {"Battle Suit", ("Icons/Items/envirosuit.svg", "White")},
            {"Shard Armor", ("Icons/Items/iconr_shard.svg", "White")},
            {"Flight", ("Icons/Items/flight.svg", "White")},
            {"Haste", ("Icons/Items/haste.svg", "White")},
            {"Health", ("Icons/Items/iconh_red.svg", "White")},
            {"Large health", ("Icons/Items/iconh_yellow.svg", "White")},
            {"Mega health", ("Icons/Items/iconh_mega.svg", "White")},
            {"Small health", ("Icons/Items/iconh_green.svg", "White")},
            {"Invisibility", ("Icons/Items/invis.svg", "White")},
            {"Quad-damage", ("Icons/Items/quad.svg", "White")},
            {"Regeneration", ("Icons/Items/regen.svg", "White")},
            {"Personal Teleporter", ("Icons/Items/teleporter.svg", "White")},
            {"Medikit", ("Icons/Items/medkit.svg", "White")},
            {"Ammo Regen (Team Arena)", ("Icons/Items/ammo_regen.svg", "White")},
            {"Scout (Team Arena)", ("Icons/Items/scout.svg", "White")},
            {"Doubler (Team Arena)", ("Icons/Items/doubler.svg", "White")},
            {"Guard (Team Arena)", ("Icons/Items/guard.svg", "White")},
            {"Kamikaze (Team Arena)", ("Icons/Items/kamikaze.svg", "White")},
            {"Invulnerability (Team Arena)", ("Icons/Items/invulnerability.svg", "White")},
            {"Green Armor (CPMA)", ("Icons/Items/iconr_green.svg", "White")}
        };

        public static readonly Dictionary<string, (string path, string color)> Functions = new Dictionary<string, (string, string)>
        {
            {"Door/Gate", ("Icons/Functions/door.svg", "White")},
            {"Button", ("Icons/Functions/button.svg", "White")},
            {"Teleporter/Portal", ("Icons/Functions/tele.svg", "White")},
            {"Jumppad/Launchramp", ("Icons/Functions/push.svg", "White")},
            {"Moving object/platform", ("Icons/Functions/moving2.svg", "White")},
            {"Shooter Grenade", ("Icons/Functions/shootergl.svg", "White")},
            {"Shooter Plasma", ("Icons/Functions/shooterpg.svg", "White")},
            {"Shooter Rocket", ("Icons/Functions/shooterrl.svg", "White")},
            {"Slick", ("Icons/Functions/slick.svg", "White")},
            {"Water", ("Icons/Functions/water.svg", "White")},
            {"Fog", ("Icons/Functions/fog.svg", "White")},
            {"Slime", ("Icons/Functions/slime.svg", "White")},
            {"Lava", ("Icons/Functions/lava.svg", "White")},
            {"breakable", ("Icons/Functions/break.svg", "White")},
            {"Ambient sounds", ("Icons/Functions/Speaker_Icon.svg", "White")},
            {"Timer", ("Icons/Functions/timer2.svg", "White")}
        };
    }
}
