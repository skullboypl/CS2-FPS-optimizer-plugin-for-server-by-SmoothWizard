using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SmoothWizardOptimizer
{
    public class SmoothWizardOptimizer : BasePlugin
    {
        public override string ModuleName => "SmoothWizard Server Optimizer";
        public override string ModuleVersion => "1.0.1";
        public override string ModuleAuthor => "SkullMedia Artur Spychalski";

        private bool isOptimizationEnabled = true;

        // List of entities to clean up
        private readonly string[] entitiesToCleanup =
        {
           "cs_ragdoll",
           "env_explosion",
           "env_fire",
           "env_spark",
           "env_smokestack",
           "info_particle_system",
           "particle_*",
           "decals_*",
           "prop_physics_multiplayer",
           "prop_physics",
           // "prop_dynamic", // WENTS, WOOD ETC
           // "prop_dynamic_ornament" // SAME AS ABOVE
        };

        // Set to FALSE for optimization to be enabled by default.
        private readonly bool debugMode = false;
        private bool clearLoopActive = false;
        private readonly HashSet<uint> removedDebugEntityIds = new();

        // Protection against the ambiguity of the name "Timer"
        private CounterStrikeSharp.API.Modules.Timers.Timer? debugTimer = null;

        private Queue<CEntityInstance> entitiesToClearQueue = new Queue<CEntityInstance>();

        public override void Load(bool hotReload)
        {
            RegisterEventHandler<EventRoundStart>(OnRoundStart);
            AddCommand("css_sw_toggle", "Toggles the SmoothWizard server optimization cleanup on/off.", OnToggleOptimizerCommand);
            AddCommand("css_sw_clear", "Starts the entity removal loop from the list. DEBUG", OnClearEntitiesLoopCommand);
            AddCommand("css_sw_stop", "Stops the entity removal loop. DEBUG", OnStopClearLoopCommand);

            Server.PrintToConsole($"[SmoothWizard Server Optimizer] Loaded successfully (v{ModuleVersion}). Optimization: {(isOptimizationEnabled ? "ON" : "OFF")}.");
        }

        public override void Unload(bool hotReload)
        {
            debugTimer?.Kill();
        }

        private HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info)
        {
            if (isOptimizationEnabled && !debugMode)
            {
                CleanupMapAndTempEntities("RoundStart");
            }
            return HookResult.Continue;
        }
        private void OnToggleOptimizerCommand(CCSPlayerController? player, CommandInfo info)
        {
            isOptimizationEnabled = !isOptimizationEnabled;
            string statusColor = isOptimizationEnabled ? ChatColors.Green.ToString() : ChatColors.Red.ToString();
            string statusText = isOptimizationEnabled ? "ON" : "OFF";

            string chatMessage = $"[{ChatColors.Green}SW Optimizer{ChatColors.White}] Server map optimizer is now: {statusColor}{statusText}{ChatColors.White}";
            //Server.PrintToChatAll(chatMessage);
            info.ReplyToCommand($"{chatMessage}");
        }

        private void OnStopClearLoopCommand(CCSPlayerController? player, CommandInfo info)
        {
            clearLoopActive = false;
            debugTimer?.Kill();
            debugTimer = null;
            entitiesToClearQueue.Clear();
            info.ReplyToCommand("Stopped entity removal debug loop (css_sw_dclear).");
            Server.PrintToConsole("[SW DEBUG] Stopped entity removal loop.");
        }

        private void OnClearEntitiesLoopCommand(CCSPlayerController? player, CommandInfo info)
        {
            if (clearLoopActive)
            {
                info.ReplyToCommand("The loop is already running. Use css_sw_dstop to stop.");
                return;
            }

            // 1. Clear and populate the queue with entities to be removed
            entitiesToClearQueue.Clear();
            foreach (var pattern in entitiesToCleanup)
            {
                var entities = Utilities.FindAllEntitiesByDesignerName<CEntityInstance>(pattern).Where(e => e != null && e.IsValid);
                foreach (var ent in entities)
                {
                    bool isProtected = ent.DesignerName.Contains("door") || ent.DesignerName.Contains("breakable") || ent.DesignerName.StartsWith("weapon_");
                    if (!isProtected)
                    {
                        entitiesToClearQueue.Enqueue(ent);
                    }
                }
            }

            if (entitiesToClearQueue.Count == 0)
            {
                info.ReplyToCommand("No entities found for removal in the debug loop list.");
                return;
            }

            clearLoopActive = true;
            info.ReplyToCommand($"[{ChatColors.Red}SmoothWizard{ChatColors.White}]Started removing {entitiesToClearQueue.Count} entities. Use css_sw_dstop to stop.");
           // Server.PrintToConsole($"[SW DEBUG] Started removing {entitiesToClearQueue.Count} entities.");

            // 2. Start the safe, repeatable CSS timer
            debugTimer = AddTimer(0.01f, ClearEntitiesLoop, TimerFlags.REPEAT);
        }

        private void ClearEntitiesLoop()
        {
            if (!clearLoopActive || entitiesToClearQueue.Count == 0)
            {
                clearLoopActive = false;
                debugTimer?.Kill();
                debugTimer = null;
                entitiesToClearQueue.Clear();
               // Server.PrintToConsole("[SW DEBUG] Entity removal loop finished.");
                Server.PrintToChatAll($"[{ChatColors.Red}SmoothWizard{ChatColors.White}] Entity removal loop finished.");
                return;
            }

            var ent = entitiesToClearQueue.Dequeue();

            if (ent == null || !ent.IsValid)
            {
                return;
            }

            Server.NextFrame(() =>
            {
                try
                {
                    string details = $"{ent.DesignerName} [ID: {ent.Index}]";
                    ent.Remove();
                    removedDebugEntityIds.Add(ent.Index); // Dodaj ID do historii usuniętych
                   // Server.PrintToConsole($"[SW DEBUG REMOVED] - {details}");
                   // Server.PrintToChatAll($"[{ChatColors.Red}SW CLEARLOOP{ChatColors.White}] Removed: {ChatColors.Red}{details}");
                }
                catch (Exception ex)
                {
                    Server.PrintToConsole($"[SW DEBUG ERROR] Error removing {ent.DesignerName} [ID: {ent.Index}]: {ex.Message}");
                }
            });
        }


        // MODIFIED CLEANUP FUNCTION FOR CORRECT LOGGING
        private void CleanupMapAndTempEntities(string context)
        {
            // The removedTotal variable is available in the closure
            int removedTotal = 0;

            // Variable to track the number of batches that need to be processed.
            int totalBatches = 0;

            foreach (var pattern in entitiesToCleanup)
            {
                var entities = Utilities.FindAllEntitiesByDesignerName<CEntityInstance>(pattern).ToList();

                foreach (var batch in entities.Chunk(50))
                {
                    totalBatches++;

                    // Each batch is removed in the next server frame
                    Server.NextFrame(() =>
                    {
                        foreach (var ent in batch)
                        {
                            try
                            {
                                if (ent == null || !ent.IsValid)
                                    continue;

                                // CRITICAL PROTECTION
                                bool isProtected = ent.DesignerName.Contains("door") || ent.DesignerName.Contains("breakable") || ent.DesignerName.StartsWith("weapon_") ||
                                                     ent.DesignerName.Contains("vent") || ent.DesignerName.Contains("shield") || ent.DesignerName.Contains("movable_platform") ||
                                                     ent.DesignerName.Contains("parent");

                                // Remove only if not protected
                                if (!isProtected)
                                {
                                    ent.Remove();
                                    // Asynchronous increment
                                    removedTotal++;
                                }
                            }
                            catch (Exception ex)
                            {
                                Server.PrintToConsole($"[SmoothWizard] Warning: failed to process {ent?.DesignerName ?? "unknown"} - {ex.Message}");
                            }
                        }

                        // Decrement the counter of processed batches
                        totalBatches--;

                        // Key condition: if all batches have been processed, we log
                        if (totalBatches == 0)
                        {
                            // Logging is delayed until the last batch (for all patterns)
                            // has been processed by Server.NextFrame.
                            if (removedTotal > 0)
                            {
                                Console.WriteLine($"[SmoothWizard Optimizer] [{context}] Cleared. Total entities removed: {removedTotal}");
                                string chatMessage = $"[{ChatColors.Red}SmoothWizard Server Optimizer{ChatColors.White}] Cleanup completed. Removed {ChatColors.Green}{removedTotal}{ChatColors.White} map junk/effects.";
                                Server.PrintToChatAll(chatMessage);
                            }
                        }
                    });
                }
            }
        }
    }
}