using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace LeveMore
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/

        /*********
        ** Public methods
        *********/
        /// <summary>
        /// The mod entry point, called after the mod is first loaded.
        /// </summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.DayEnding += this.OnDayEnd;
        }

        /*********
        ** Private methods
        *********/
        private int CalcLevel(int exp)
        {
            double test = 2.545 * Math.Log(exp / 294.627);
            return (int) test;
        }
        private int GetLevel(int skill)
        {
            Farmer player = Game1.player;
            switch (skill)
            {
                case Farmer.farmingSkill:
                    return player.FarmingLevel;
                case Farmer.fishingSkill:
                    return player.FishingLevel;
                case Farmer.foragingSkill:
                    return player.ForagingLevel;
                case Farmer.miningSkill:
                    return player.MiningLevel;
                case Farmer.combatSkill:
                    return player.CombatLevel;
            }
            return -1;
        }
        private void SetLevel(int skill, int level)
        {
            Farmer player = Game1.player;
            switch (skill)
            {
                case Farmer.farmingSkill:
                    player.FarmingLevel = level;
                    break;
                case Farmer.fishingSkill:
                    player.FishingLevel = level;
                    break;
                case Farmer.foragingSkill:
                    player.ForagingLevel = level;
                    break;
                case Farmer.miningSkill:
                    player.MiningLevel = level;
                    break;
                case Farmer.combatSkill:
                    player.CombatLevel = level;
                    break;
            }
        }
        private void UpdateLevel(int skill)
        {
            Farmer player = Game1.player;
            if (player.experiencePoints[skill] > 15000)
            {
                int CurrLevel = GetLevel(skill);
                int NewLevel = CalcLevel(player.experiencePoints[skill]);
                if (NewLevel > CurrLevel)
                {
                    SetLevel(skill, NewLevel);
                    player.newLevels.Add(new Point(skill, NewLevel));
                }
            }
        }
        private void OnDayEnd(object sender, DayEndingEventArgs e)
        {
            UpdateLevel(Farmer.farmingSkill);
            UpdateLevel(Farmer.fishingSkill);
            UpdateLevel(Farmer.foragingSkill);
            UpdateLevel(Farmer.miningSkill);
            UpdateLevel(Farmer.combatSkill);
        }
    }
}
