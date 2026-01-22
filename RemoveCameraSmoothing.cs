using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

[BepInPlugin("repo.removecamerasmoothing", "Remove Camera Smoothing", "1.2.0")]
public class RemoveCameraSmoothing : BaseUnityPlugin
{
    private void Awake()
    {
        Harmony harmony = new Harmony("repo.removecamerasmoothing");
        harmony.PatchAll();
        Logger.LogInfo("Remove Camera Smoothing loaded.");
    }
}

[HarmonyPatch(typeof(CameraAim), "Update")]
class CameraAim_Update_Patch
{
    static FieldInfo playerAimField =
        AccessTools.Field(typeof(CameraAim), "playerAim");

    static void Postfix(CameraAim __instance)
    {
        // ===== ① 主菜单 / UI 场景：绝对不碰 =====
        if (SemiFunc.MenuLevel())
            return;

        // ===== ② 非玩家相机（主菜单动画 / 演出）=====
        if (CameraNoPlayerTarget.instance != null)
            return;

        // ===== ③ GameDirector 尚未准备好 =====
        if (GameDirector.instance == null)
            return;

        // ===== ④ 还没进入 Main =====
        if (GameDirector.instance.currentState < GameDirector.gameState.Main)
            return;

        // ===== ⑤ 相机被禁止输入（演出、控制锁定）=====
        if (GameDirector.instance.DisableInput)
            return;

        if (playerAimField == null)
            return;

        // ===== ⑥ 只在“玩家正常操控相机”时生效 =====
        Quaternion playerAim =
            (Quaternion)playerAimField.GetValue(__instance);

        __instance.transform.localRotation = playerAim;
    }
}
