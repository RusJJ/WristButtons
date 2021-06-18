using UnityEngine;
using UnityEngine.UI;

namespace WristButtons
{
    public class Plane
    {
        public static GameObject plane = null;
        internal static GameObject plane_confirmation = null;
        internal static WristButton lastButtonPressed = null;
        internal static void CreatePlane()
        {
            plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(plane.GetComponent<Rigidbody>());
            GameObject.Destroy(plane.GetComponent<BoxCollider>());
            GameObject.Destroy(plane.GetComponent<MeshRenderer>()); // Because... we dont really need it.
            plane.transform.localScale = new Vector3(0.001f, 0.2f, 0.2f);
            plane.SetActive(false);

            plane.transform.position = WristObjects.centerObject.transform.position;
            plane.transform.rotation = WristObjects.centerObject.transform.rotation;
            plane.transform.Rotate(0.0f, -90.0f, 0.0f);
            plane.transform.parent = WristObjects.centerObject.transform;
        }
        internal static void CreateConfirmationPlane()
        {
            plane_confirmation = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(plane_confirmation.GetComponent<Rigidbody>());
            GameObject.Destroy(plane_confirmation.GetComponent<BoxCollider>());
            GameObject.Destroy(plane_confirmation.GetComponent<MeshRenderer>()); // Because... we dont really need it.
            plane_confirmation.transform.localScale = new Vector3(0.001f, 0.2f, 0.2f);
            plane_confirmation.SetActive(false);

            plane_confirmation.transform.position = WristObjects.centerObject.transform.position;
            plane_confirmation.transform.rotation = WristObjects.centerObject.transform.rotation;
            plane_confirmation.transform.Rotate(0.0f, -90.0f, 0.0f);
            plane_confirmation.transform.parent = WristObjects.centerObject.transform;
        }
        internal static WristButton BuildConfirmationButton(string text)
        {
            WristButton newButton = new WristButton();
            newButton.body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(newButton.body.GetComponent<Rigidbody>());
            BoxCollider col = newButton.body.GetComponent<BoxCollider>();
            col.isTrigger = true;
            col.gameObject.layer = WristButton.layerForTrigger;
            newButton.body.transform.localScale = new Vector3(0.01f, 0.023f, 0.14f);
            newButton.body.transform.localPosition = Vector3.zero;
            newButton.body.transform.rotation = plane_confirmation.transform.rotation;
            newButton.body.transform.position = plane_confirmation.transform.position;
            newButton.body.transform.parent = plane_confirmation.transform;
            newButton.body.AddComponent<WristButtonCollider>().pairedButton = newButton;
            newButton.body.GetComponent<Renderer>().material.SetColor("_Color", WristButton.colorDefault);

            newButton.textGO = new GameObject();
            newButton.textGO.transform.position = plane_confirmation.transform.position;
            newButton.textGO.transform.parent = WristButtons.canvasObjectConfirm.transform;
            newButton.textObject = newButton.textGO.AddComponent<Text>();

            newButton.textObject.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            newButton.textObject.text = text;
            newButton.textObject.fontSize = 1;
            newButton.textObject.alignment = TextAnchor.MiddleCenter;
            newButton.textObject.resizeTextForBestFit = false;
            newButton.textObject.resizeTextMinSize = 0;
            newButton.textObject.transform.localScale = WristButton.buttonSizeRegular;

            RectTransform rectTransform = newButton.textObject.GetComponent<RectTransform>();
            newButton.textObject.transform.rotation = plane_confirmation.transform.rotation;
            newButton.textObject.transform.Rotate(0.0f, 90.0f, 0.0f);
            newButton.textObject.transform.localPosition = WristButton.buttonTextPosOffset;

            return newButton;
        }
    }
}