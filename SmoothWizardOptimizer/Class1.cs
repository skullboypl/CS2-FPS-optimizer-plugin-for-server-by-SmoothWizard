using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Commands; 
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmoothWizardOptimizer
{
    public class SmoothWizardOptimizer : BasePlugin
    {
        public override string ModuleName => "SmoothWizard Server Optimizer";
        public override string ModuleVersion => "1.0.0"; 
        public override string ModuleAuthor => "SkullMedia Artur Spychalski";

        private bool isOptimizationEnabled = true;

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
            "prop_dynamic",
            "prop_dynamic_ornament"
        };

        // Debug mode: logs only, does not remove
        private readonly bool debugMode = false;

        public override void Load(bool hotReload)
        {
            RegisterEventHandler<EventRoundStart>(OnRoundStart);
            AddCommand("css_sw_toggle", "Toggles the SmoothWizard server optimization cleanup on/off.", OnToggleOptimizerCommand);
            Server.PrintToConsole("[SmoothWizard Server Optimizer] Loaded successfully (v1.0.0). Optimization is ON. Debug mode: " + debugMode);

            if (debugMode)
            {
                ListAllEntitiesToConsole();
            }
        }

        private HookResult OnRoundStart(EventRoundStart ev, GameEventInfo info)
        {
            if (isOptimizationEnabled)
            {
                CleanupMapAndTempEntities("RoundStart");
            }
            return HookResult.Continue;
        }

        // COMMAND TO ON OFF OPTIMIZER css_sw_toggle
        private void OnToggleOptimizerCommand(
            CCSPlayerController? player,
            CommandInfo info)
        {
            isOptimizationEnabled = !isOptimizationEnabled;
            string status = isOptimizationEnabled ? $"{ChatColors.Green}ON" : $"{ChatColors.Red}OFF";
            Server.PrintToChatAll($"{ChatColors.Green}[SW Optimizer] {ChatColors.White}Server map optimizer is now: {status}");
            info.ReplyToCommand($"Server map optimizer is now: {status}");
        }

        // Consolidated cleanup function
        private void CleanupMapAndTempEntities(string context)
        {
            int ragdollCount = 0;
            int particleEffectCount = 0;
            int mapJunkCount = 0;
            int removedTotal = 0;

            foreach (var pattern in entitiesToCleanup)
            {
                var entities = Utilities.FindAllEntitiesByDesignerName<CEntityInstance>(pattern).ToList();

                foreach (var batch in entities.Chunk(50))
                {
                    foreach (var ent in batch)
                    {
                        try
                        {
                            if (ent == null || !ent.IsValid)
                                continue;

                            // Skip essential entities (e.g., doors, breakables, weapons)
                            if (ent.DesignerName.Contains("door") || ent.DesignerName.Contains("breakable") || ent.DesignerName.StartsWith("weapon_"))
                                continue;

                            if (!debugMode)
                                ent.Remove();

                            removedTotal++;

                            // Categorization for display in the chat message:
                            if (ent.DesignerName.Contains("ragdoll"))
                                ragdollCount++;
                            else if (ent.DesignerName.Contains("particle") || ent.DesignerName.StartsWith("env_"))
                                particleEffectCount++;
                            else
                                mapJunkCount++;
                        }
                        catch (Exception ex)
                        {
                            Server.PrintToConsole($"[SmoothWizard] Warning: failed to remove {ent?.DesignerName ?? "unknown"} - {ex.Message}");
                        }
                    }
                }
            }

            if (!debugMode && removedTotal > 0)
            {
                Server.PrintToChatAll($"{ChatColors.Green}[SW Optimizer] {ChatColors.White}Cleaned: {ChatColors.Green}{ragdollCount}{ChatColors.White} ragdolls, {ChatColors.Green}{particleEffectCount}{ChatColors.White} particles/effects, {ChatColors.Green}{mapJunkCount}{ChatColors.White} map junk.");
            }

            Console.WriteLine($"[SmoothWizard Optimizer] [{context}] Cleared: {ragdollCount} ragdolls, {particleEffectCount} effects, {mapJunkCount} map junk. Total: {removedTotal}");
        }

        // Function to list entities (for debug mode)
        private void ListAllEntitiesToConsole()
        {
            Server.PrintToConsole("[SmoothWizard] DEBUG: Listing all entities by DesignerName:");

            var allEntities = Utilities.FindAllEntitiesByDesignerName<CEntityInstance>("").ToList();
            var uniqueNames = new HashSet<string>();

            foreach (var ent in allEntities)
            {
                if (ent != null && ent.IsValid)
                {
                    uniqueNames.Add(ent.DesignerName);
                }
            }

            int count = 0;
            foreach (var name in uniqueNames.OrderBy(n => n))
            {
                Server.PrintToConsole($"[SmoothWizard] Entity [{count++}]: {name}");
            }

            Server.PrintToConsole($"[SmoothWizard] DEBUG: Total unique entity names: {uniqueNames.Count}");
        }
    }
}