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
            int SkillCode = this.ExpArgs(command, args);
            if (SkillCode == -1)
                return;
        }
        private void SetExp(string command, string[] args)
        {
            return;
        }
        private int ExpArgs(string command, string[] args)
        {
            if (args.Length != 2)
            {
                this.Monitor.Log($"{command} recieved {args.Length} args instead of 2!");
                return -1;
            }
            string skill = args[0];
            if (skill.Contains("farm"))
                return 0;
            else if (skill.Contains("min"))
                return 1;
            else if (skill.Contains("fish"))
                return 2;
            else if (skill.Contains("for"))
                return 3;
            else if (skill.Contains("com"))
                return 4;
            this.Monitor.Log($"{skill} is not a valid skill");
            return -1;
        }
    }
}

