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
    [BepInPlugin(ModConstants.ModConstants.modGUID, ModConstants.ModConstants.modName, ModConstants.ModConstants.modVersion)]
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
        internal static WristButton __disconnect = null;
        internal static WristButton __show_scoreboard = null;

        internal static WristButton __confirmation_title = null;
        internal static WristButton __confirmation_yes = null;
        internal static WristButton __confirmation_no = null;

        internal static SphereCollider leftHandToucher = null;
        internal static SphereCollider rightHandToucher = null;

        internal static bool scoreboardOpened = false;
        internal static GameObject wristScoreBoardAnchor = null;
        internal static GorillaScoreboardSpawner wristScoreBoardSpawner = null;

        private static WristButtons m_hInstance;
        internal static void Log(string msg) => m_hInstance.Logger.LogMessage(msg);
        void Awake()
        {
            m_hInstance = this;
            HarmonyPatcher.Patch.Apply(ModConstants.ModConstants.modGUID);
        }
        internal static void Thread_EveryHalfMinute()
        {
            while(true)
            {
                __show_time.SetText(DateTime.Now.ToString("hh:mm tt"));
                Thread.Sleep(1800); // 0.5 minutes
            }
        }
        internal static void DisconnectHandler(WristButton b)
        {
            PhotonNetworkController.instance.AttemptDisconnect();
        }
        internal static void OpenScoreboard(WristButton b)
        {
            Plane.plane.SetActive(false);
            WristButtons.scoreboardOpened = true;
        }
    }

    [HarmonyPatch(typeof(OVRManager))]
    [HarmonyPatch("Update", MethodType.Normal)]
    internal class UnityDebug2
    {
        /* OVRManager::Update flooding... */
        public static bool Prefix()
        {
            return false;
        }
    }


    [HarmonyPatch(typeof(GorillaLocomotion.Player))]
    [HarmonyPatch("Awake", MethodType.Normal)]
    internal class PlayerAwake
    {
        public static GameObject rhSphere = null;
        private static void Postfix()
        {
            /* Get controller devices */
            InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller, WristButtons.list);
            WristButton.buttons = new List<WristButton>();
            
            /* Get out fingers */
            var tmp = GameObject.Find("RightHandTriggerCollider");
            if(tmp != null) WristButtons.rightHandToucher = tmp.GetComponent<SphereCollider>();
            tmp = GameObject.Find("LeftHandTriggerCollider");
            if(tmp != null) WristButtons.leftHandToucher = tmp.GetComponent<SphereCollider>();

            /* Create planes for button placement */
            WristObjects.CreateCenterObject();
            Plane.CreatePlane();
            Plane.CreateConfirmationPlane();

            /* Create scoreboard */
            var anchor = GameObject.Find("Photon Manager/GorillaUI/ForestScoreboardAnchor");
            if (anchor != null)
            {
                WristButtons.wristScoreBoardAnchor = GameObject.Instantiate(anchor, Plane.plane.transform.position, Plane.plane.transform.rotation);
                WristButtons.wristScoreBoardAnchor.name = "WristButtonsScoreboardAnchor";
                WristButtons.wristScoreBoardAnchor.transform.parent = GameObject.Find("Photon Manager/GorillaUI").transform;
                WristButtons.wristScoreBoardAnchor.AddComponent<AnchorUpdater>().anchor = WristButtons.wristScoreBoardAnchor;
                WristButtons.wristScoreBoardAnchor.transform.localScale = new Vector3(0.2f, 0.2f, 0.1f);
                WristButtons.wristScoreBoardSpawner = WristButtons.wristScoreBoardAnchor.GetComponent<GorillaScoreboardSpawner>();
                WristButtons.wristScoreBoardSpawner.controllingParentGameObject = WristObjects.centerObject;
            }

            /* Text drawer for main plane! */
            WristButtons.canvasObject = new GameObject();
            WristButtons.canvasObject.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            WristButtons.canvasObject.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 100;
            WristButtons.canvasObject.AddComponent<GraphicRaycaster>();
            WristButtons.canvasObject.transform.parent = Plane.plane.transform;

            /* Text drawer for confirmation plane! */
            WristButtons.canvasObjectConfirm = new GameObject();
            WristButtons.canvasObjectConfirm.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            WristButtons.canvasObjectConfirm.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 100;
            WristButtons.canvasObjectConfirm.AddComponent<GraphicRaycaster>();
            WristButtons.canvasObjectConfirm.transform.parent = Plane.plane_confirmation.transform;

            /* Buttons&Text for confirmation plane! */
            WristButtons.__confirmation_title = Plane.BuildConfirmationButton("Title");
            WristButtons.__confirmation_title.DisableBox();
            WristButtons.__confirmation_title.SetLocalPosition(new Vector3(0.0f, 0.4f, 0.0f));
            WristButtons.__confirmation_yes = Plane.BuildConfirmationButton("Yes");
            WristButtons.__confirmation_yes.SetLocalPosition(new Vector3(0.0f, 0.2f, 0.36f));
            WristButtons.__confirmation_no = Plane.BuildConfirmationButton("No");
            WristButtons.__confirmation_no.SetLocalPosition(new Vector3(0.0f, 0.2f, -0.36f));

            /* BUTTONS! */
            WristButtons.__show_time = WristButton.CreateButton("__show_time", "00:00 AM");
            WristButtons.__show_time.DisableBox();
            new Thread(WristButtons.Thread_EveryHalfMinute).Start();

            WristButtons.__show_other = WristButton.CreateButton("__show_other", ModConstants.ModConstants.modName + " v" + ModConstants.ModConstants.modVersion + "\nBy [-=KILL MAN=-] aka RusJJ");
            WristButtons.__show_other.DisableBox();
            WristButtons.__show_other.SmallerFont();

            WristButtons.__disconnect = WristButton.CreateButton("__disconnect", "Disconnect");
            WristButtons.__disconnect.requireConfirmation = true;
            WristButtons.__disconnect.confirmationTitle = "Are you sure you want to disconnect?";
            WristButtons.__disconnect.action = WristButtons.DisconnectHandler;

            WristButtons.__show_scoreboard = WristButton.CreateButton("__scoreboard", "Open Scoreboard");
            WristButtons.__show_scoreboard.action = WristButtons.OpenScoreboard;

            /* Functions Regular/Post (i dont want to provide PRE!) */
            foreach (var plugin in Chainloader.PluginInfos.Values)
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
        internal static Vector3 justAHeadPositionForTimeSave = new Vector3();
        private static void Postfix(GorillaLocomotion.Player __instance)
        {
            if (WristButtons.list == null || WristButtons.list.Count < 1) return; // OpenXR failed to init

            /* Is grip pressed? */
            WristButtons.list[0].TryGetFeatureValue(CommonUsages.gripButton, out WristButtons.gripNow);
            if (WristButtons.gripNow != WristButtons.gripPrev)
            {
                WristButtons.gripPrev = WristButtons.gripNow;
                Plane.plane.SetActive(WristButtons.gripNow);
                Plane.plane_confirmation.SetActive(false);
                WristButtons.scoreboardOpened = false;
            }

            /* Repositioning of Center Object (plane's parent) */
            WristObjects.centerObject.transform.position = GorillaTagger.Instance.leftHandTransform.position + WristObjects.centerHeightOffset;
            WristObjects.centerObject.transform.rotation = GorillaTagger.Instance.bodyCollider.transform.rotation;

            /* Update buttons on background */
            if (!Plane.plane.activeSelf)
            {
                foreach (var b in WristButton.buttons)
                {
                    b.body.GetComponent<WristButtonCollider>().Update();
                }
            }
        }
    }
    /* Scoreboard updater */
    public class AnchorUpdater : MonoBehaviour
    {
        internal GameObject anchor = null;
        internal Vector3 heightPos = new Vector3(0.0f, 0.3f, 0.0f);
        void Update()
        {
            if (!Plane.plane.activeSelf && WristButtons.scoreboardOpened)
            {
                anchor.transform.position = GorillaTagger.Instance.bodyCollider.transform.position + 0.7f * GorillaTagger.Instance.bodyCollider.transform.forward +
                                            0.05f * GorillaTagger.Instance.bodyCollider.transform.right + heightPos;
                anchor.transform.rotation = GorillaTagger.Instance.bodyCollider.transform.rotation;
            }
            else
            {
                anchor.transform.position = Vector3.zero;
            }
        }
    }
}