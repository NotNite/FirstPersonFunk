using HarmonyLib;
using Reptile;
using UnityEngine;

namespace FirstPersonFunk.Patches;

[HarmonyPatch(typeof(GameplayCamera))]
public class GameplayCameraPatch {
    private static Vector2 Offset = new(0, 0);

    [HarmonyPrefix]
    [HarmonyPatch("UpdateCamera")]
    public static bool UpdateCamera(GameplayCamera __instance) {
        __instance.UpdateCameraInput();

        var tf = __instance.transform;
        var head = __instance.player.headTf;
        tf.position = head.position;

        var input = __instance.orbitInput *
                    (__instance.mouseForOrbit ? __instance.mouseFactor : __instance.joystickFactor);

        if (Plugin.RotateWithHead.Value) {
            Offset.x += input.x;
            Offset.y += input.y;
            Offset.y = Clamp(Offset.y);

            var offsetQuat = Quaternion.Euler(new Vector3(Offset.y, Offset.x, 0));
            // Pretend the head bone is rotated
            var fakeHead = head.rotation * Quaternion.Euler(new Vector3(0, 0, 90));
            tf.rotation = fakeHead * offsetQuat;
        } else {
            var euler = tf.rotation.eulerAngles;
            euler.x += input.y;
            euler.y += input.x;
            euler.z = 0;
            euler.x = Clamp(euler.x);

            tf.rotation = Quaternion.Euler(euler);
        }

        return false;
    }

    // Clamp code I stole from BombRushCamera because it's 7 AM and I forgot how this works
    private static float Clamp(float wtf) {
        wtf = wtf > 180 ? wtf - 360 : wtf;
        wtf = Mathf.Clamp(wtf, -89, 89);
        return (wtf + 360) % 360;
    }
}
