using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace LevelSpeed
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>
        /// Configured parameter that sets up our speed scaling
        /// </summary>
        private int speedScale;
        /// <summary>
        /// The last addedSpeed we set
        /// </summary>
        private int lastAddedSpeed = -1;
        /// <summary>
        /// If we were walking on last update
        /// </summary>
        private bool wasWalking = true;
        /// <summary>
        /// If we were on the horse at last update
        /// </summary>
        private bool lastWasRiding = false;

        /*********
        ** Public methods
        *********/
        /// <summary>
        /// The mod entry point, called after the mod is first loaded.
        /// </summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Unpack config
            ModConfig Config = helper.ReadConfig<ModConfig>();
            this.speedScale = Config.speedScale;

            // Register events
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>
        /// Update on tick if the player's addedSpeed was changed by other code,
        /// if horse state changes, or player switches between walk/run.
        /// Don't update on every single tick to minimize load.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // Skip if we haven't started yet
            if (!Context.IsWorldReady)
                return;

            Farmer player = Game1.player;
            if ((player.addedSpeed != this.lastAddedSpeed) || (player.isRidingHorse() != this.lastWasRiding) || (player.running == this.wasWalking))
            {
                this.UpdatePlayerSpeed();
            }
        }

        /// <summary>
        /// Always update if we're starting a day
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            this.UpdatePlayerSpeed();
        }

        /// <summary>
        /// Update the player's addedSpeed value based on existing buffs and the player's total level
        /// </summary>
        private void UpdatePlayerSpeed()
        {
            // Grab farmer to reduce typing
            Farmer player = Game1.player;

            // Calculate the "base" buff from our player's total level (5-50)
            // At total level 0, we should have no extra buff, but at 50 we should max out
            int totalLevel = player.FarmingLevel + player.MiningLevel + player.FishingLevel
                             + player.ForagingLevel + player.CombatLevel;
            float baseBuff = this.speedScale * totalLevel / 50;
            float addedSpeed = baseBuff;

            // Calculate multiplier for existing buffs and horse. If player is going 2x speed, buffs should too
            float mult = (baseBuff + 5) / 5;

            // Check applied speed buff (private field)
            Netcode.NetArray<int, Netcode.NetInt> appliedBuffs = this.Helper.Reflection.GetField<Netcode.NetArray<int, Netcode.NetInt>>(player, "appliedBuffs").GetValue();
            int appliedSpeedBuff = appliedBuffs[9];
            // Add it to our sum, scaled by the multplier
            addedSpeed += appliedSpeedBuff * mult;

            // Check horse state, and add it based on our multiplier
            if (player.isRidingHorse())
            {
                addedSpeed += 5 * mult;
                this.lastWasRiding = true;
            }
            else
                this.lastWasRiding = false;

            // Decide what to set (don't add speed if walking)
            int addedSpeedToUse;
            if (player.running || player.isRidingHorse())
            {
                addedSpeedToUse = (int)addedSpeed;
                this.wasWalking = false;
            }
            else
            {
                addedSpeedToUse = appliedSpeedBuff;
                this.wasWalking = true;
            }
            player.addedSpeed = addedSpeedToUse;

            this.lastAddedSpeed = addedSpeedToUse;

            this.Monitor.Log($"Set player.addedSpeed to {player.addedSpeed}");
        }
    }
}

/* Primer on how player speed works based on testing/reading source:
 - Whenever we want to update the player's position, Farmer.getMovementSpeed() is called
   - Farmer.getMovementSpeed() returns a value, different if we're in a control sequence or in an event
   - Farmer.getMovementSpeed() will always return at least one, or at least 0.7 if moving diagonally
     (compensates for "moving two directions at once" effect)
 - In normal game loop, the formula is proportional to:
   -> player.speed + player.addedSpeed + 4.6 if on a horse, or player.temporarySpeedBuff if not
 - player.speed is the player speed constant
   - it is adjusted by the game when:
     1. mounting a horse (set to 2)
     2. dismounting a horse (set to 5 if running, 2 if not)
     3. starting/stopping running (5 or 2)
     4. sometimes it oscillates between 5 and 2 while walking???
   - it is also used separately from getMovementSpeed() in the update position loop...
 - player.addedSpeed is typically used by buffs/debuffs to adjust player speed
   - it is adjusted by the game when:
     1. a buff/debuff is applied/wears off (increase/decrease value appropriately)
     2. at the start of the day (set to zero)
     3. every 10 in-game minutes if no recorded food, drink, or "other" buffs (set to zero)
  - this.temporarySpeedBuff is set to zero immediately after calling getMovementSpeed

  It seems most prudent to mess with player.addedSpeed because player.speed is being repeatedly
  set, and is used by the game in difficult to parse ways. Our goal is then to:
  - set player.addedSpeed to something proportional to total level while running
  - do not disrupt food/potion buffs (in fact, enhance them proportionally)
  - increase speed even more if we're on a horse (we don't want horse to be useless)
  The idea is to make the player feel powerful through leveling up their character without
  "outmoding" existing game mechanics that increase player speed.

  There aren't specific events for the triggers we want (on/off horse, got a buff...) so we'll
  update on game ticks, but skip the calculation when appropriate.

  We also calculate on day start, or else we're moving normal speed as we get out of bed.

  TODO: make the "max speed" configurable, check for bugs during events/scenes
*/
