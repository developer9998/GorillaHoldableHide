using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GorillaHoldableHide.Models;
using HarmonyLib;
using System.Reflection;

namespace GorillaHoldableHide;

[BepInPlugin("dev.gorillaholdablehide", "GorillaHoldableHide", "1.0.0")]
[BepInDependency("dev.gorillalibrary")]
internal class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;

    public static new ConfigFile Config;

    public static ConfigEntry<bool> AffectLocal, AffectOthers;

    public static ConfigEntry<HoldableVisibility> ArmVisibility, BackVisibility, ChestVisibility;

    public void Awake()
    {
        Logger = base.Logger;
        Config = base.Config;

        AffectLocal = Config.Bind("Player", "Affect Local Player", true, "Whether holdables belonging to the local player are hidden");
        AffectOthers = Config.Bind("Player", "Affect Other Players", false, "Whether holdables belonging to other players are hidden");
        ArmVisibility = Config.Bind("Category", "Arm Visibility", HoldableVisibility.TPHidden, "The visibility of holdables bound to your arms");
        BackVisibility = Config.Bind("Category", "Back Visibility", HoldableVisibility.TPHidden, "The visibility of holdables bound to your back");
        ChestVisibility = Config.Bind("Category", "Chest Visibility", HoldableVisibility.Hidden, "The visibility of holdables bound to your chest");

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Info.Metadata.GUID);
    }
}
