using BepInEx;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

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
        public static WristButton __gravity = null;
        public static WristButton __tag_target = null;
        private static Main m_hInstance;
        internal static void Log(string msg) => m_hInstance.Logger.LogMessage(msg);
        void Awake()
        {
            m_hInstance = this;
            Patch.Apply();

            BananaHook.Events.OnRoundStart += OnRoundStart;
            BananaHook.Events.OnPlayerTagPlayer += OnPlayerTagged;
            BananaHook.Events.OnRoomJoined += OnRoomJoined;
            BananaHook.Events.OnRoomDisconnected += OnRoomQuit;
            BananaHook.Events.OnPlayerConnected += OnPlayerJoin;
        }
        private static void OnReadyForButtons()
        {
            __last_tagged = WristButton.CreateButtonAfter("__show_other", "__last_tagged", "Last Tagged:\nNo info");
            __last_tagged.DisableBox();
            __last_tagged.SmallerFont();
            __room_code = WristButton.CreateButtonAfter(__last_tagged, "__room_code", "Room Code: Not joined");
            __room_code.DisableBox();
            __gravity = WristButton.CreateButton("__gravity", "Gravity", WristButton.ButtonType.Toggleable);
            __gravity.actionToggled = OnGravityChanged;
            __tag_target = WristButton.CreateButton("__tag_target", "Tag Target:\nEmpty", WristButton.ButtonType.Switchable);
            __tag_target.requireConfirmation = true;
            __tag_target.SmallerFont();
            __tag_target.actionSwitched = OnTagTargetSwitch;
            __tag_target.action = TagTarget;
            __tag_target.Block();
        }
        private static void OnTagTargetSwitch(WristButton b, bool left)
        {
            if (b.ownObject == null) return;
            var players = Photon.Pun.PhotonNetwork.PlayerList;
            if (players.Length == 1) return;
            int index = 0;
            foreach(var p in players)
            {
                if (p == b.ownObject) break;
                ++index;
            }
            if(left)
            {
                --index;
                if (index < 0) index = players.Length - 1;
            }
            else
            {
                ++index;
                if (index >= players.Length) index = 0;
            }
            b.ownObject = players[index];
            __tag_target.SetText("Tag Target:\n" + ((Player)b.ownObject).NickName);
            __tag_target.confirmationTitle = "Are you sure you want to tag " + players[index].NickName + "?";
        }
        private static void TagTarget(WristButton b)
        {
            if (b.ownObject == null)
            {
                __tag_target.ownObject = Photon.Pun.PhotonNetwork.PlayerList[0];
                __tag_target.SetText("Tag Target:\n" + ((Player)__tag_target.ownObject).NickName);
                __tag_target.confirmationTitle = "Are you sure you want to tag " + ((Player)__tag_target.ownObject).NickName + "?";
            }
            PhotonView.Get(GorillaTagManager.instance.GetComponent<GorillaGameManager>()).RPC("ReportTagRPC", RpcTarget.MasterClient, new object[]{b.ownObject});
        }
        private static void OnGravityChanged(WristButton b, bool enabled)
        {
            if(enabled)
            {
                b.SetText("NoGravity");
                GorillaTagger.Instance.bodyCollider.attachedRigidbody.useGravity = false;
                return;
            }
            b.SetText("Gravity");
            GorillaTagger.Instance.bodyCollider.attachedRigidbody.useGravity = true;
        }
        private static void OnRoundStart(object sender, BananaHook.OnRoundStartArgs e)
        {
            __last_tagged.SetText("Last Tagged:\n" + e.player.NickName + " on start");
        }
        private static void OnPlayerTagged(object sender, BananaHook.PlayerTaggedPlayerArgs e)
        {
            __last_tagged.SetText("Last Tagged:\n" + e.victim.NickName + " by " + e.tagger.NickName);
        }
        private static void OnRoomJoined(object sender, BananaHook.RoomJoinedArgs e)
        {
            __tag_target.ownObject = Photon.Pun.PhotonNetwork.PlayerList[0];
            __tag_target.SetText("Tag Target:\n" + ((Player)__tag_target.ownObject).NickName);
            __tag_target.confirmationTitle = "Are you sure you want to tag " + ((Player)__tag_target.ownObject).NickName + "?";
            __room_code.SetText("Room Code: " + e.roomCode);
            __last_tagged.SetText("Last Tagged:\n" + Photon.Pun.PhotonNetwork.LocalPlayer.NickName + " on join");
            if (!e.isPrivate)
            {
                __gravity.Block();
                __tag_target.Block();
            }
            else
            {
                __tag_target.Unblock();
            }
        }
        private static void OnRoomQuit(object sender, EventArgs e)
        {
            __room_code.SetText("Room Code: Not joined");
            __last_tagged.SetText("Last Tagged:\nNo info");
            __gravity.Unblock();
            __tag_target.ownObject = null;
            __tag_target.SetText("Tag Target:\nEmpty");
            __tag_target.Block();
        }
        private static void OnPlayerJoin(object sender, BananaHook.PlayerDisConnectedArgs e)
        {
            __last_tagged.SetText("Last Tagged:\n" + e.player.NickName + " on join");
        }
        private static void DisconnectHandler(WristButton b)
        {
            PhotonNetworkController.instance.AttemptDisconnect();
        }
    }
}