# 🚀 SmoothWizard Server Optimizer (CSSHarp)

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

### Installation Steps
1.  Download the latest version of the plugin (`SmoothWizardOptimizer.dll`) from the [Releases](https://github.com/TwojaNazwaUżytkownika/NazwaRepozytorium/releases) section.
2.  Create a folder named `SmoothWizardOptimizer` inside your CSSHarp plugins directory:
    ```
    /game/csgo/addons/counterstrikesharp/plugins/SmoothWizardOptimizer/
    ```
3.  Place the downloaded `SmoothWizardOptimizer.dll` file inside this new folder.
4.  Start (or restart) your CS2 server.

## 🖥️ Command to disable optimizer or enable

The plugin uses the CSSHarp permission system for feature control.

| Command | Description | Required Permission |
| :--- | :--- | :--- |
| `css_sw_toggle` | Toggles the automatic entity cleanup on or off or on chat !sw_toggle
