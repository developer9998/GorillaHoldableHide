using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace GorillaHoldableHide;

[BepInPlugin("dev.gorillaholdablehide", "GorillaHoldableHide", "1.0.0")]
[BepInDependency("dev.gorillalibrary")]
internal class Plugin : BaseUnityPlugin
{
    public static new ManualLogSource Logger;

    public static new ConfigFile Config;

    public static ConfigEntry<bool> AffectLocal, AffectOthers, RegisterArm, RegisterBack, RegisterChest, ShowThirdPerson;

    public void Awake()
    {
        Logger = base.Logger;
        Config = base.Config;

        AffectLocal = Config.Bind("Player", "Affect Local Player", true, "Whether holdables belonging to the local player are hidden");
        AffectOthers = Config.Bind("Player", "Affect Other Players", false, "Whether holdables belonging to other players are hidden");
        RegisterArm = Config.Bind("Category", "Affect Arm Cosmetics", true, "Whether holdables that fit on the arm are hidden");
        RegisterBack = Config.Bind("Category", "Affect Back Cosmetics", true, "Whether holdables that fit on the back are hidden");
        RegisterChest = Config.Bind("Category", "Affect Chest Cosmetics", true, "Whether holdables that fit on the chest are hidden");
        ShowThirdPerson = Config.Bind("Perspective", "Show Outside POV", false, "Whether holdables remain solely hidden through your point of view");

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Info.Metadata.GUID);
    }
}
