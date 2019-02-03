using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayModeGUI : MonoBehaviour
{
    public PlayMode PlayMode;
    public Toggle ViewGameMasterToggle;

    GraphicRaycaster raycaster;
    EventSystem eventSystem;
    PointerEventData pointerEventData;
    bool isInteractingWithUI = false;

    private void Start()
    {
        raycaster = FindObjectOfType<GraphicRaycaster>();
        eventSystem = FindObjectOfType<EventSystem>();
    }

    private void Update()
    {
        pointerEventData = new PointerEventData(eventSystem);
        //Set the Pointer Event Position to that of the mouse position
        pointerEventData.position = Input.mousePosition;

        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        raycaster.Raycast(pointerEventData, results);

        isInteractingWithUI = results.Count > 0;
    }

    public bool GetIsInteractingWithUI()
    {
        return isInteractingWithUI;
    }
}
