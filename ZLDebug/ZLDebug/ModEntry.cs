using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ZLDebug
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
            helper.ConsoleCommands.Add("gain_exp", "Gains a value of exp.\n\nUsage: gain_exp <skill> <value>", this.GainExp);
            helper.ConsoleCommands.Add("set_exp", "Sets exp to a value.\n\nUsage: set_exp <skill> <value>", this.SetExp);
        }

        /*********
        ** Private methods
        *********/
        private void GainExp(string command, string[] args)
        {
            Tuple<int, int> argints = this.ExpArgs(command, args);
            int skillnum = argints.Item1;
            int expnum = argints.Item2;
            if (skillnum == -1 || expnum == -1)
                return;

            Game1.player.gainExperience(skillnum, expnum);
        }
        private void SetExp(string command, string[] args)
        {
            Tuple<int, int> argints = this.ExpArgs(command, args);
            int skillnum = argints.Item1;
            int expnum = argints.Item2;
            if (skillnum == -1 || expnum == -1)
                return;

            Farmer player = Game1.player;
            player.experiencePoints[skillnum] = 0;
            switch (skillnum)
            {
                case 0:
                    player.FarmingLevel = 0;
                    break;
                case 1:
                    player.FishingLevel = 0;
                    break;
                case 2:
                    player.ForagingLevel = 0;
                    break;
                case 3:
                    player.MiningLevel = 0;
                    break;
                case 4:
                    player.CombatLevel = 0;
                    break;
            }
            player.gainExperience(skillnum, expnum);
        }
        private Tuple<int, int> ExpArgs(string command, string[] args)
        {
            if (args.Length != 2)
            {
                this.Monitor.Log($"{command} recieved {args.Length} args instead of 2!");
                return Tuple.Create(-1, -1);
            }
            string skill = args[0];
            string expstr = args[1];
            int skillnum = -1;
            int expnum = -1;

            if (!int.TryParse(expstr, out expnum))
                this.Monitor.Log($"{expstr} cannot be parsed as an int");

            if (skill.Contains("farm"))
                skillnum = 0;
            else if (skill.Contains("fish"))
                skillnum = 1;
            else if (skill.Contains("for"))
                skillnum = 2;
            else if (skill.Contains("min"))
                skillnum = 3;
            else if (skill.Contains("com"))
                skillnum = 4;
            else
                this.Monitor.Log($"{skill} is not a valid skill");

            return Tuple.Create(skillnum, expnum);
        }
    }
}

