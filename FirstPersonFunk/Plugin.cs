using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Reptile;

namespace FirstPersonFunk;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("Bomb Rush Cyberfunk.exe")]
public class Plugin : BaseUnityPlugin {
    public static ManualLogSource Log = null!;
    public static Harmony Harmony = null!;
    public static ConfigEntry<bool> ShowPlayer = null!;
    public static ConfigEntry<bool> RotateWithHead = null!;

    private void Awake() {
        Log = this.Logger;
        Harmony = new Harmony("FirstPersonFunk.Harmony");
        Harmony.PatchAll();

        ShowPlayer = this.Config.Bind(
            "General",
            "ShowPlayer",
            true,
            "Whether to show the player character in first person mode."
        );
        RotateWithHead = this.Config.Bind(
            "General",
            "RotateWithHead",
            false,
            "Whether to rotate the camera with the player's head, or to only move it when you move it."
        );
    }

    private void Update() {
        if (ShowPlayer.Value) return;
        if (WorldHandler.instance == null) return;
        var player = WorldHandler.instance.GetCurrentPlayer();
        if (player == null) return;
        player.characterVisual.mainRenderer.enabled = player.cam == null;
    }
}
