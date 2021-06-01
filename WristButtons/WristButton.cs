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
            RegularConfirmation = 1,
            Toggleable = 2,
            Switchable = 3,
            SwitchableConfirmation = 4,
        }
        public const int layerForTrigger = 11;
        public const float buttonReleaseTime = 1.2f;
        public const float buttonsGapOffset = 0.16f;// = 0.125f;
        public static readonly Color colorDefault = new Color(0.7f, 0.7f, 0.7f, 1.0f);
        public static readonly Color colorPressed = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        public static readonly Color colorBlocked = new Color(0.4f, 0.2f, 0.2f, 1.0f);
        public static readonly Color colorEnabled = new Color(0.0f, 0.5f, 0.0f, 1.0f);
        public static readonly Vector3 buttonSizeRegular = new Vector3(0.011f, 0.018f, 1.0f);
        public static readonly Vector3 buttonSizeSmaller = new Vector3(0.007f, 0.01f, 1.0f);
        internal static List<WristButton> buttons = new List<WristButton>();
        internal static Vector3 buttonTextPosOffset = new Vector3(0.006f, 0.0f, 0.0f);

        internal GameObject body = null;
        internal GameObject textBody = null;
        internal GameObject textGO = null;
        internal Text textObject = null;
        //internal Text textObjectOutline = null;
        internal ButtonType type = ButtonType.Regular;
        internal bool isToggled = false;
        internal bool isBlocked = false;
        internal string myId = "";
        internal float pressTime = 0.0f;

        public object ownObject = null;
        public string confirmationTitle = null;
        public string confirmationYes = null;
        public string confirmationNo = null;
        public Action<WristButton> action = null;
        public Action<WristButton, bool> actionToggled = null;
        private static WristButton BuildButton(string myId, string text, ButtonType type)
        {
            WristButton newButton = new WristButton();
            newButton.myId = myId;
            newButton.type = type;
            newButton.body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(newButton.body.GetComponent<Rigidbody>());
            BoxCollider col = newButton.body.GetComponent<BoxCollider>();
            col.isTrigger = true;
            col.gameObject.layer = layerForTrigger;
            newButton.body.transform.localScale = new Vector3(0.01f, 0.023f, 0.14f);
            newButton.body.transform.localPosition = Vector3.zero;
            newButton.body.transform.rotation = Plane.plane.transform.rotation;
            newButton.body.transform.position = Plane.plane.transform.position;
            newButton.body.transform.parent = Plane.plane.transform;
            newButton.body.AddComponent<WristButtonCollider>().pairedButton = newButton;
            newButton.body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorDefault);

            // Create the Text GameObject.
            newButton.textGO = new GameObject();
            newButton.textGO.transform.position = Plane.plane.transform.position;
            newButton.textGO.transform.parent = WristButtons.canvasObject.transform;
            //Outline outline = newButton.textGO.AddComponent<Outline>();
            newButton.textObject = newButton.textGO.AddComponent<Text>();

            // Set Text component properties.
            newButton.textObject.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            newButton.textObject.text = text;
            newButton.textObject.fontSize = 1;
            newButton.textObject.alignment = TextAnchor.MiddleCenter;
            newButton.textObject.resizeTextForBestFit = false;
            newButton.textObject.resizeTextMinSize = 0;
            newButton.textObject.transform.localScale = buttonSizeRegular;

            // Provide Text position and size using RectTransform.
            RectTransform rectTransform = newButton.textObject.GetComponent<RectTransform>();
            newButton.textObject.transform.rotation = Plane.plane.transform.rotation;
            newButton.textObject.transform.Rotate(0.0f, 90.0f, 0.0f);
            newButton.textObject.transform.localPosition = buttonTextPosOffset;

            newButton.confirmationTitle = text + "?";
            newButton.confirmationYes = "Yes";
            newButton.confirmationNo = "No";

            return newButton;
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
                //b.textGO.GetComponent<Outline>().transform.position = b.body.transform.position - Plane.plane.transform.right * buttonTextPosOffset.x;
                b.textObject.transform.position = b.body.transform.position - Plane.plane.transform.right * buttonTextPosOffset.x;
                //b.textObjectOutline.transform.position = b.body.transform.position - 1.1f * Plane.plane.transform.right * buttonTextPosOffset.x;
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
        }
        public void Unblock()
        {
            isBlocked = false;
            body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorDefault);
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
    }
    internal class WristButtonCollider : MonoBehaviour
    {
        internal WristButton pairedButton = null;
        private void Update()
        {
            if(pairedButton.type != WristButton.ButtonType.Toggleable && pairedButton.pressTime > 0.0f)
            {
                if(pairedButton.pressTime < Time.time)
                {
                    if(!pairedButton.isBlocked) pairedButton.body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorDefault);
                    pairedButton.pressTime = 0.0f;
                }
            }
        }
        private void OnTriggerEnter(Collider collider)
        {
            if (pairedButton.isBlocked || WristButtons.pressCooldown > Time.time || collider.transform != GorillaButtonClicker.toucher.transform || pairedButton.pressTime > 0.0f) return;
            WristButtons.pressCooldown = Time.time + WristButtons.cooldownTime;
            GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tapHapticDuration);

            if (pairedButton == WristButtons.__confirmation_yes || pairedButton == WristButtons.__confirmation_no)
            {
                if(pairedButton == WristButtons.__confirmation_yes)
                {
                    Plane.lastButtonPressed.action?.Invoke(Plane.lastButtonPressed);
                }
                Plane.plane.SetActive(true);
                Plane.plane_confirmation.SetActive(false);
                return;
            }

            if (pairedButton.type == WristButton.ButtonType.Toggleable)
            {
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
            pairedButton.pressTime = Time.time + WristButton.buttonReleaseTime;
            pairedButton.body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorPressed);
            if(pairedButton.type == WristButton.ButtonType.RegularConfirmation)
            {
                Plane.plane.SetActive(false);
                Plane.lastButtonPressed = pairedButton;
                WristButtons.__confirmation_title.SetText(pairedButton.confirmationTitle);
                WristButtons.__confirmation_yes.SetText(pairedButton.confirmationYes);
                WristButtons.__confirmation_no.SetText(pairedButton.confirmationNo);
                Plane.plane_confirmation.SetActive(true);
            }
            else
            {
                pairedButton.action?.Invoke(pairedButton);
            }
        }
    }
    internal class GorillaButtonClicker
    {
        internal static GameObject toucher = null;
        public static void CreateToucher()
        {
            if(toucher == null)
            {
                toucher = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GameObject.Destroy(toucher.GetComponent<MeshRenderer>());
            }
            toucher.transform.position = GorillaTagger.Instance.rightHandTransform.position;
            toucher.transform.parent = GorillaTagger.Instance.rightHandTransform;
            toucher.transform.localPosition = new Vector3(-0.008f, -0.09f, 0.02f);
            toucher.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            toucher.GetComponent<SphereCollider>().gameObject.layer = WristButton.layerForTrigger;
            toucher.SetActive(false);
        }
    }
}