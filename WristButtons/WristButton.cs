using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WristButtons
{
    public class WristButton
    {
        public enum ButtonType : byte
        {
            Regular = 0,
            Toggleable = 1,
            Switchable = 2,
        }
        public const int layerForTrigger = 18; // 11
        public const float cooldownTime = 0.15f;
        public const float buttonReleaseTime = 0.5f;
        public const float buttonsGapOffset = 0.16f;// = 0.125f;
        public static readonly Color colorDefault = new Color(0.7f, 0.7f, 0.7f, 1.0f);
        public static readonly Color colorPressed = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        public static readonly Color colorBlocked = new Color(0.4f, 0.2f, 0.2f, 1.0f);
        public static readonly Color colorEnabled = new Color(0.0f, 0.5f, 0.0f, 1.0f);
        public static readonly Vector3 buttonSizeRegular = new Vector3(0.011f, 0.018f, 1.0f);
        public static readonly Vector3 buttonSizeSmaller = new Vector3(0.007f, 0.01f, 1.0f);
        internal static List<WristButton> buttons = null;
        internal static Vector3 buttonTextPosOffset = new Vector3(0.007f, 0.0f, 0.0f);

        internal GameObject body = null;
        internal GameObject textBody = null;
        internal GameObject textGO = null;
        internal Text textObject = null;
        internal Rigidbody attachedRigidbody = null;
        internal ButtonType type = ButtonType.Regular;
        internal bool isToggled = false;
        internal bool isBlocked = false;
        internal string myId = "";
        internal float pressTime = 0.0f;
        internal WristButton[] myArrowButtons = null;

        public object ownObject = null;
        public bool   requireConfirmation = false;
        public string confirmationTitle = null;
        public string confirmationYes = null;
        public string confirmationNo = null;
        public Action<WristButton> action = null;
        public Action<WristButton, bool> actionToggled = null;
        public Action<WristButton, bool> actionSwitched = null;
        private static WristButton BuildButton(string myId, string text, ButtonType type)
        {
            WristButton newButton = new WristButton();
            newButton.myId = myId;
            newButton.type = type;
            newButton.body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newButton.body.AddComponent<WristButtonInfo>().pairedButton = newButton;
            BoxCollider col = newButton.body.GetComponent<BoxCollider>();
            col.isTrigger = true;
            col.gameObject.layer = layerForTrigger;
            newButton.body.transform.localScale = new Vector3(0.01f, 0.023f, 0.14f);
            newButton.body.transform.localPosition = Vector3.zero;
            if (type == ButtonType.Switchable)
            {
                newButton.body.transform.localScale = new Vector3(0.01f, 0.023f, 0.093f);
                newButton.myArrowButtons = new WristButton[2];
                newButton.myArrowButtons[0] = WristButton.BuildButton("", "<", ButtonType.Regular);
                newButton.myArrowButtons[0].body.transform.parent = newButton.body.transform;
                newButton.myArrowButtons[0].body.transform.localScale = new Vector3(0.8f, 0.9f, 0.242f);
                newButton.myArrowButtons[0].body.transform.localPosition = new Vector3(0.0f, 0.0f, 0.62f);
                newButton.myArrowButtons[0].body.transform.Rotate(0.0f, -90.0f, 0.0f);
                newButton.myArrowButtons[0].ownObject = newButton;
                newButton.myArrowButtons[0].action = OnLeftRightBtnPress;
                newButton.myArrowButtons[1] = WristButton.BuildButton("", ">", ButtonType.Regular);
                newButton.myArrowButtons[1].body.transform.parent = newButton.body.transform;
                newButton.myArrowButtons[1].body.transform.localScale = new Vector3(0.8f, 0.9f, 0.242f);
                newButton.myArrowButtons[1].body.transform.localPosition = new Vector3(0.0f, 0.0f, -0.62f);
                newButton.myArrowButtons[1].body.transform.Rotate(0.0f, -90.0f, 0.0f);
                newButton.myArrowButtons[1].ownObject = newButton;
                newButton.myArrowButtons[1].action = OnLeftRightBtnPress;
            }
            newButton.body.transform.rotation = Plane.plane.transform.rotation;
            newButton.body.transform.position = Plane.plane.transform.position;
            newButton.body.transform.parent = Plane.plane.transform;
            newButton.body.AddComponent<WristButtonCollider>().pairedButton = newButton;
            newButton.body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorDefault);

            // Create the Text GameObject.
            newButton.textGO = new GameObject();
            newButton.textGO.transform.position = Plane.plane.transform.position;
            newButton.textGO.transform.parent = WristButtons.canvasObject.transform;
            newButton.textObject = newButton.textGO.AddComponent<Text>();

            // Set Text component properties.
            newButton.textObject.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            newButton.textObject.text = text;
            newButton.textObject.fontSize = 1;
            newButton.textObject.alignment = TextAnchor.MiddleCenter;
            newButton.textObject.resizeTextForBestFit = false;
            newButton.textObject.resizeTextMinSize = 0;
            newButton.textObject.transform.localScale = buttonSizeRegular;

            newButton.textObject.transform.rotation = Plane.plane.transform.rotation;
            newButton.textObject.transform.Rotate(0.0f, 90.0f, 0.0f);
            newButton.textObject.transform.localPosition = buttonTextPosOffset;

            newButton.confirmationTitle = text + "?";
            newButton.confirmationYes = "Yes";
            newButton.confirmationNo = "No";

            return newButton;
        }
        private static void OnLeftRightBtnPress(WristButton b)
        {
            ((WristButton)b.ownObject).actionSwitched?.Invoke((WristButton)b.ownObject, b.GetText() == "<");
        }
        public static WristButton CreateButton(string myId, string text, ButtonType type = ButtonType.Regular)
        {
            foreach (var b in buttons)
            {
                if(b.myId == myId) return b;
            }
            WristButton newButton = BuildButton(myId, text, type);
            buttons.Add(newButton);
            RebuildButtons();
            return newButton;
        }
        public static WristButton CreateButtonAfter(WristButton afterBtn, string myId, string text, ButtonType type = ButtonType.Regular)
        {
            foreach (var b in buttons)
            {
                if(b.myId == myId) return b;
            }
            WristButton newButton = BuildButton(myId, text, type);
            int index = 1;
            foreach (var b in buttons)
            {
                if (b == afterBtn)
                {
                    buttons.Insert(index, newButton);
                    index = -1;
                    break;
                }
                ++index;
            }
            if (index != -1) buttons.Add(newButton);
            RebuildButtons();
            return newButton;
        }
        public static WristButton CreateButtonBefore(WristButton afterBtn, string myId, string text, ButtonType type = ButtonType.Regular)
        {
            foreach (var b in buttons)
            {
                if(b.myId == myId) return b;
            }
            WristButton newButton = BuildButton(myId, text, type);
            int index = 0;
            foreach (var b in buttons)
            {
                if (b == afterBtn)
                {
                    buttons.Insert(index, newButton);
                    index = -1;
                    break;
                }
                ++index;
            }
            if (index != -1) buttons.Add(newButton);
            RebuildButtons();
            return newButton;
        }
        public static WristButton CreateButtonAfter(string afterId, string myId, string text, ButtonType type = ButtonType.Regular)
        {
            foreach (var b in buttons)
            {
                if(b.myId == myId) return b;
            }
            WristButton newButton = BuildButton(myId, text, type);
            int index = 1;
            foreach (var b in buttons)
            {
                if (b.myId == afterId)
                {
                    buttons.Insert(index, newButton);
                    index = 0;
                    break;
                }
                ++index;
            }
            if (index != 0) buttons.Add(newButton);
            RebuildButtons();
            return newButton;
        }
        public static WristButton CreateButtonBefore(string afterId, string myId, string text, ButtonType type = ButtonType.Regular)
        {
            foreach (var b in buttons)
            {
                if(b.myId == myId) return b;
            }
            WristButton newButton = BuildButton(myId, text, type);
            int index = 0;
            foreach (var b in buttons)
            {
                if (b.myId == afterId)
                {
                    buttons.Insert(index, newButton);
                    index = -1;
                    break;
                }
                ++index;
            }
            if (index != -1) buttons.Add(newButton);
            RebuildButtons();
            return newButton;
        }
        public static bool RemoveButton(string btnId)
        {
            foreach(var b in buttons)
            {
                if(b.myId == btnId)
                {
                    GameObject.Destroy(b.textGO);
                    GameObject.Destroy(b.body);
                    buttons.Remove(b);
                    RebuildButtons();
                    return true;
                }
            }
            return false;
        }
        public static bool RemoveButton(WristButton btn)
        {
            if (btn == null) return false;
            GameObject.Destroy(btn.textGO);
            GameObject.Destroy(btn.body);
            bool ret = buttons.Remove(btn);
            RebuildButtons();
            return ret;
        }
        public static void RebuildButtons()
        {
            int localButtonsCount = 0;
            float off = buttonsGapOffset * Convert.ToSingle(Math.Ceiling(buttons.Count * 0.5f));
            bool btnLeft = false;
            foreach (var b in buttons)
            {
                ++localButtonsCount;
                Vector3 pos = b.body.transform.localPosition;
                if (!btnLeft)
                {
                    if (localButtonsCount != buttons.Count)
                    {
                        pos.z = 0.357f;
                    }
                    pos.y = off;
                }
                else
                {
                    pos.z = -0.357f;
                    pos.y = off;
                    off -= buttonsGapOffset;
                }
                b.body.transform.localPosition = pos;
                b.textObject.transform.position = b.body.transform.position - Plane.plane.transform.right * buttonTextPosOffset.x;
                if(b.type == ButtonType.Switchable)
                {
                    var b1 = b.myArrowButtons[0];
                    b1.textObject.transform.position = b1.body.transform.position - Plane.plane.transform.right * buttonTextPosOffset.x;
                    b1 = b.myArrowButtons[1];
                    b1.textObject.transform.position = b1.body.transform.position - Plane.plane.transform.right * buttonTextPosOffset.x;
                }
                btnLeft = !btnLeft;
            }
        }
        public static WristButton FindById(string btnId)
        {
            foreach(var b in buttons)
            {
                if (b.myId == btnId) return b;
            }
            return null;
        }
        public void SmallerFont(bool smaller = true)
        {
            textObject.transform.localScale = smaller ? buttonSizeSmaller : buttonSizeRegular;
        }
        public void SetTextColor(Color clr)
        {
            textObject.color = clr;
        }
        public void SetText(string text)
        {
            textObject.text = text;
        }
        public string GetText()
        {
            return textObject.text;
        }
        public void DisableBox()
        {
            body.SetActive(false);
        }
        public void EnableBox()
        {
            body.SetActive(true);
            textObject.gameObject.SetActive(true);
        }
        public void DisableButton()
        {
            body.SetActive(false);
            textObject.gameObject.SetActive(false);
        }
        public void EnableButton()
        {
            body.SetActive(true);
            textObject.gameObject.SetActive(true);
        }
        public bool IsToggled()
        {
            return (type == ButtonType.Toggleable) && isToggled;
        }
        public void ToggleOff()
        {
            if(type == ButtonType.Toggleable && isToggled)
            {
                isToggled = false;
                body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorDefault);
                actionToggled?.Invoke(this, false);
            }
        }
        public void ToggleOn()
        {
            if(type == ButtonType.Toggleable && !isToggled)
            {
                isToggled = true;
                body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorEnabled);
                actionToggled?.Invoke(this, true);
            }
        }
        public void Block()
        {
            isBlocked = true;
            ToggleOff();
            body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorBlocked);
            if (this == Plane.lastButtonPressed && Plane.plane_confirmation.activeSelf)
            {
                Plane.plane.SetActive(true);
                Plane.plane_confirmation.SetActive(false);
            }
            if(type == ButtonType.Switchable)
            {
                myArrowButtons[0].Block();
                myArrowButtons[1].Block();
            }
        }
        public void Unblock()
        {
            isBlocked = false;
            body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorDefault);
            if (type == ButtonType.Switchable)
            {
                myArrowButtons[0].Unblock();
                myArrowButtons[1].Unblock();
            }
        }
        public Vector3 GetLocalPosition()
        {
            return body.transform.localPosition;
        }
        public void SetLocalPosition(Vector3 pos)
        {
            body.transform.localPosition = pos;
            textObject.transform.position = body.transform.position - Plane.plane.transform.right * buttonTextPosOffset.x;
        }
        internal void AskForConfirm()
        {
            if(requireConfirmation)
            {
                Plane.plane.SetActive(false);
                Plane.lastButtonPressed = this;
                WristButtons.__confirmation_title.SetText(this.confirmationTitle);
                WristButtons.__confirmation_yes.SetText(this.confirmationYes);
                WristButtons.__confirmation_no.SetText(this.confirmationNo);
                Plane.plane_confirmation.SetActive(true);
            }
        }
    }
    internal class WristButtonCollider : MonoBehaviour
    {
        internal WristButton pairedButton = null;
        internal void Update()
        {
            if(pairedButton.pressTime > 0.0f)
            {
                if(pairedButton.pressTime < Time.time)
                {
                    if(!pairedButton.isBlocked && pairedButton.type != WristButton.ButtonType.Toggleable) pairedButton.body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorDefault);
                    pairedButton.pressTime = 0.0f;
                }
            }
        }
        private void OnTriggerEnter(Collider collider)
        {
            if (pairedButton.isBlocked || WristButtons.pressCooldown > Time.time || collider != WristButtons.rightHandToucher || pairedButton.pressTime > 0.0f) return;
            WristButtons.pressCooldown = Time.time + WristButton.cooldownTime;
            GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tapHapticDuration);

            if (pairedButton == WristButtons.__confirmation_yes || pairedButton == WristButtons.__confirmation_no)
            {
                if(pairedButton == WristButtons.__confirmation_yes)
                {
                    if(pairedButton.type == WristButton.ButtonType.Toggleable)
                    {
                        if (Plane.lastButtonPressed.isToggled)
                        {
                            Plane.lastButtonPressed.body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorEnabled);
                            Plane.lastButtonPressed.actionToggled?.Invoke(Plane.lastButtonPressed, true);
                        }
                        else
                        {
                            Plane.lastButtonPressed.body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorDefault);
                            Plane.lastButtonPressed.actionToggled?.Invoke(Plane.lastButtonPressed, false);
                        }
                    }
                    else
                    {
                        Plane.lastButtonPressed.action?.Invoke(Plane.lastButtonPressed);
                    }
                }
                Plane.plane.SetActive(true);
                Plane.plane_confirmation.SetActive(false);
                return;
            }

            pairedButton.pressTime = Time.time + WristButton.buttonReleaseTime;
            if (pairedButton.type == WristButton.ButtonType.Toggleable)
            {
                if(pairedButton.requireConfirmation)
                {
                    pairedButton.AskForConfirm();
                    return;
                }
                pairedButton.isToggled = !pairedButton.isToggled;
                if (pairedButton.isToggled)
                {
                    pairedButton.body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorEnabled);
                    pairedButton.actionToggled?.Invoke(pairedButton, true);
                }
                else
                {
                    pairedButton.body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorDefault);
                    pairedButton.actionToggled?.Invoke(pairedButton, false);
                }
                return;
            }

            pairedButton.body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorPressed);
            if (pairedButton.requireConfirmation)
            {
                pairedButton.AskForConfirm();
            }
            else
            {
                pairedButton.action?.Invoke(pairedButton);
            }
        }
    }
    internal class WristObjects
    {
        internal static GameObject centerObject = null;
        public static readonly Vector3 centerHeightOffset = new Vector3(0.0f, 0.03f, 0.0f);
        public static readonly Vector3 toucherOffset = new Vector3(-0.008f, -0.09f, 0.02f);
        public static void CreateCenterObject()
        {
            centerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(centerObject.GetComponent<BoxCollider>());
            GameObject.Destroy(centerObject.GetComponent<MeshRenderer>());
        }
    }
    internal class WristButtonInfo : MonoBehaviour
    {
        internal WristButton pairedButton;
    }
}