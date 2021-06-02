using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace WristButtons
{
    /* That's me! */
    [BepInPlugin(Constants.modGUID, Constants.modName, Constants.modVersion)]
    public class WristButtons : BaseUnityPlugin
    {
        internal static GameObject canvasObject = null;
        internal static GameObject canvasObjectConfirm = null;
        internal static List<InputDevice> list = new List<InputDevice>();
        internal static bool gripPrev = false;
        internal static bool gripNow = false;
        internal static float pressCooldown = 0.0f;
        internal static WristButton __show_time = null;
        internal static WristButton __show_other = null;

        internal static WristButton __confirmation_title = null;
        internal static WristButton __confirmation_yes = null;
        internal static WristButton __confirmation_no = null;

        private static WristButtons m_hInstance;
        internal static void Log(string msg) => m_hInstance.Logger.LogMessage(msg);
        void Awake()
        {
            m_hInstance = this;
            Patch.Apply();
        }
        internal static void Thread_EveryMinute()
        {
            while(true)
            {
                __show_time.SetText(DateTime.Now.ToString("hh:mm tt"));
                Thread.Sleep(1800); // 0.5 minutes
            }
        }
    }

    [HarmonyPatch(typeof(GorillaLocomotion.Player))]
    [HarmonyPatch("Awake", MethodType.Normal)]
    internal class PlayerAwake
    {
        private static void Postfix()
        {
            InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller, WristButtons.list);
            GorillaButtonClicker.CreateToucher();
            Plane.CreatePlane();
            WristButtons.canvasObject = new GameObject();
            WristButtons.canvasObject.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            WristButtons.canvasObject.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 100;
            WristButtons.canvasObject.AddComponent<GraphicRaycaster>();
            WristButtons.canvasObject.transform.parent = Plane.plane.transform;
            WristButtons.__show_time = WristButton.CreateButton("__show_time", "00:00 AM");
            WristButtons.__show_time.DisableBox();
            WristButtons.__show_other = WristButton.CreateButton("__show_other", Constants.modName + " v" + Constants.modVersion + "\nBy [-=KILL MAN=-] aka RusJJ");
            WristButtons.__show_other.DisableBox();
            WristButtons.__show_other.SmallerFont();
            new Thread(WristButtons.Thread_EveryMinute).Start();

            Plane.CreateConfirmationPlane();
            WristButtons.canvasObjectConfirm = new GameObject();
            WristButtons.canvasObjectConfirm.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            WristButtons.canvasObjectConfirm.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 100;
            WristButtons.canvasObjectConfirm.AddComponent<GraphicRaycaster>();
            WristButtons.canvasObjectConfirm.transform.parent = Plane.plane_confirmation.transform;
            WristButtons.__confirmation_title = Plane.BuildConfirmationButton("Title");
            WristButtons.__confirmation_title.DisableBox();
            WristButtons.__confirmation_title.SetLocalPosition(new Vector3(0.0f, 0.4f, 0.0f));
            WristButtons.__confirmation_yes = Plane.BuildConfirmationButton("Yes");
            WristButtons.__confirmation_yes.SetLocalPosition(new Vector3(0.0f, 0.2f, 0.36f));
            WristButtons.__confirmation_no = Plane.BuildConfirmationButton("No");
            WristButtons.__confirmation_no.SetLocalPosition(new Vector3(0.0f, 0.2f, -0.36f));

            foreach(var plugin in Chainloader.PluginInfos.Values)
            {
                var onReadyFunc = AccessTools.Method(plugin.Instance.GetType(), "OnReadyForButtons");
                if(onReadyFunc != null)
                {
                    onReadyFunc.Invoke(plugin.Instance, new object[]{});
                }
            }
            foreach(var plugin in Chainloader.PluginInfos.Values)
            {
                var onReadyFunc = AccessTools.Method(plugin.Instance.GetType(), "OnReadyForButtonsPost");
                if(onReadyFunc != null)
                {
                    onReadyFunc.Invoke(plugin.Instance, new object[]{});
                }
            }
        }
    }
    [HarmonyPatch(typeof(GorillaLocomotion.Player))]
    [HarmonyPatch("Update", MethodType.Normal)]
    internal class PlayerUpdate
    {
        //internal static Vector3 planeFloatingOffset = new Vector3(0.0f, 0.08f, 0.0f);
        internal static Vector3 planeFloatingOffset = new Vector3(0.0f, 0.02f, 0.0f);
        internal static Vector3 justAHeadPositionForTimeSave = new Vector3();
        private static void Postfix(GorillaLocomotion.Player __instance)
        {
            WristButtons.list[0].TryGetFeatureValue(CommonUsages.gripButton, out WristButtons.gripNow);
            if(WristButtons.gripNow != WristButtons.gripPrev)
            {
                WristButtons.gripPrev = WristButtons.gripNow;
                Plane.plane.SetActive(WristButtons.gripNow);
                Plane.plane_confirmation.SetActive(false);
                GorillaButtonClicker.toucher.SetActive(WristButtons.gripNow);
            }
            Plane.plane.transform.position = __instance.leftHandTransform.position + planeFloatingOffset;
            justAHeadPositionForTimeSave.x = __instance.headCollider.transform.position.x;
            justAHeadPositionForTimeSave.y = Plane.plane.transform.position.y;
            justAHeadPositionForTimeSave.z = __instance.headCollider.transform.position.z;
            Plane.plane.transform.LookAt(justAHeadPositionForTimeSave);
            Plane.plane.transform.Rotate(0.0f, 90.0f, 0.0f);

            Plane.plane_confirmation.transform.position = Plane.plane.transform.position;
            Plane.plane_confirmation.transform.rotation = Plane.plane.transform.rotation;

            if(!Plane.plane.activeSelf)
            {
                foreach(var b in WristButton.buttons)
                {
                    b.body.GetComponent<WristButtonCollider>().Update();
                }
            }
        }
    }
}