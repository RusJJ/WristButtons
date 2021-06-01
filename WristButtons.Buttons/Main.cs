using BepInEx;
using System;

namespace WristButtons.Buttons
{
    /* That's me! */
    [BepInPlugin("net.rusjj.wristbuttons.buttons", "WristButtons: Buttons", "1.0.0")]
    /* WristButtons: Why are we using it? This is so simple */
    [BepInDependency("net.rusjj.wristbuttons", "1.0.0")]
    /* BananaHook: Our little API */
    [BepInDependency("net.rusjj.gtlib.bananahook", "1.0.1")]
    public class Main : BaseUnityPlugin
    {
        public static WristButton __last_tagged = null;
        public static WristButton __room_code = null;
        public static WristButton __disconnect = null;
        public static WristButton __gravity = null;
        void Awake()
        {
            BananaHook.Events.OnRoundStart += OnRoundStart;
            BananaHook.Events.OnPlayerTagPlayer += OnPlayerTagged;
            BananaHook.Events.OnRoomJoined += OnRoomJoined;
            BananaHook.Events.OnRoomDisconnected += OnRoomQuit;
        }
        private void OnReadyForButtons()
        {
            __last_tagged = WristButton.CreateButton("__last_tagged", "Last Tagged:\nNone");
            __last_tagged.DisableBox();
            __last_tagged.SmallerFont();
            __room_code = WristButton.CreateButton("__room_code", "Room Code: Not joined");
            __room_code.DisableBox();
            __disconnect = WristButton.CreateButton("__disconnect", "Disconnect", WristButton.ButtonType.RegularConfirmation);
            __disconnect.confirmationTitle = "Are you sure you want to disconnect?";
            __disconnect.action = DisconnectHandler;
            __disconnect.Block();
            __gravity = WristButton.CreateButton("__gravity", "Gravity", WristButton.ButtonType.Toggleable);
            __gravity.actionToggled = OnGravityChanged;
        }
        private static void OnGravityChanged(WristButton b, bool enabled)
        {
            GorillaTagger.Instance.bodyCollider.attachedRigidbody.useGravity = !enabled;
        }
        private static void OnRoundStart(object sender, BananaHook.OnRoundStartArgs e)
        {
            __last_tagged.SetText("Last Tagged:\n" + e.player.NickName + " by server");
        }
        private static void OnPlayerTagged(object sender, BananaHook.PlayerTaggedPlayerArgs e)
        {
            __last_tagged.SetText("Last Tagged:\n" + e.victim.NickName + " by " + e.tagger.NickName);
        }
        private static void OnRoomJoined(object sender, BananaHook.RoomJoinedArgs e)
        {
            __room_code.SetText("Room Code: " + e.roomCode);
            __last_tagged.SetText("Last Tagged:\n" + Photon.Pun.PhotonNetwork.LocalPlayer.NickName + " by server");
            if (!e.isPrivate) __gravity.Block();
            __disconnect.Unblock();
        }
        private static void OnRoomQuit(object sender, EventArgs e)
        {
            __room_code.SetText("Room Code: Not joined");
            __last_tagged.SetText("Last Tagged:\nNone");
            __gravity.Unblock();
            __disconnect.Block();
        }
        private static void DisconnectHandler(WristButton b)
        {
            PhotonNetworkController.instance.AttemptDisconnect();
        }
    }
}