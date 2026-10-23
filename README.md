# 🚀 SmoothWizard Server Optimizer (CSSHarp)
# [INFO WEBSITE (EN/PL)](https://smoothwizard.com/cs2-fps-optimization-plugin-smoothwizard-server-cleaner-reduce-lag/)

## About the Project

**SmoothWizard Server Optimizer** is a high-performance plugin built on the **CounterStrikeSharp (CSSHarp)** framework, designed to significantly boost server stability and improve client performance (FPS) in Counter-Strike 2.

The plugin employs an aggressive **entity cleanup** strategy at the start of every round, eliminating stagnant and resource-intensive objects that often lead to framerate drops and server instability on busy maps.

## ✨ Key Features

* **Automatic Per-Round Cleanup:** Automatically removes excessive objects and effects from the map at the start of every round.
* **High Efficiency:** Specifically targets common causes of performance degradation:
    * **Ragdolle** (Player bodies).
    * **Particles and Effects** (explosions, smoke, fire, `env_explosion`, `info_particle_system`).
    * **Decals** (Bullet holes, blood splatters, sprays).
    * **Map Junk** (`prop_physics`, bottles, cans, dynamic props).
* **Clear Notifications:** Provides a consolidated, colorful chat notification showing players the total number of entities cleaned up.

## ⚙️ Installation

### Prerequisites
1.  **Metamod:Source** installed for CS2.
2.  **CounterStrikeSharp (CSSHarp)** framework installed.

### Before and after on Train 
<img width="330" height="109" alt="567069473_1475568960377879_9192262773945323278_n" src="https://github.com/user-attachments/assets/a2f5941b-080f-46f4-abc2-859496613cff" />
<img width="320" height="62" alt="566555109_1159235275631818_439753922021541332_n" src="https://github.com/user-attachments/assets/c9a1d0ae-f51d-4699-9c18-6eb31ca53027" />

| 100fps more because the plugin removes rain particles 

### Sample stuff that plugin removes
<img width="330" height="auto" alt="JUNK1" src="https://smoothwizard.com/wp-content/uploads/2025/10/MedalTVCounterStrike220251027204002-768x432.png" />
<img width="320" height="auto" alt="JUNK1" src="https://smoothwizard.com/wp-content/uploads/2025/10/MedalTVCounterStrike220251027204023-768x432.png" />

### Installation Steps
1.  Download the latest version of the plugin (`SmoothWizardOptimizer.dll`) from the [Releases](https://github.com/skullboypl/CS2-FPS-optimizer-plugin-for-server-by-SmoothWizard/releases) section.
2.  Create a folder named `SmoothWizardOptimizer` inside your CSSHarp plugins directory:
    ```
    /game/csgo/addons/counterstrikesharp/plugins/SmoothWizardOptimizer/
    ```
3.  Place `SmoothWizardOptimizer.dll` in that folder, and copy the `lang` folder next to it (`lang/en.json`). Chat lines live in that file. Add another `lang/<code>.json` (same keys) if you want a different language. The RCON line `smoothwizard_state on` / `smoothwizard_state off` stays in English so panels can read it.
4.  Start (or restart) your CS2 server.

## 🖥️ Command to disable optimizer or enable

The plugin uses the CSSHarp permission system for feature control.

### Commands

| Command | Description |
| :--- | :--- |
| `css_sw_on` | Turns automatic cleanup on. Reply includes `smoothwizard_state on`. |
| `css_sw_off` | Turns automatic cleanup off. Reply includes `smoothwizard_state off`. |
| `css_sw_status` | Prints the current state as `smoothwizard_state on` or `smoothwizard_state off`. |
| `css_sw_toggle` | Toggles automatic cleanup (`!sw_toggle` in chat). Reply includes the same state line. |
| `css_sw_clear` | Starts the entity removal loop (debug mode). |
| `css_sw_stop` | Stops the entity removal loop (debug mode). |
