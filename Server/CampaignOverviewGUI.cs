using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CampaignOverviewGUI : MonoBehaviour {

    const string Instruction1 = "(Click level tiles to select them.)";
    const string Instruction2 = "(Click level tiles one by one in their order in the campaign.)";

    public Button EditButton;
    public Button DeleteButton;
    public Text DeleteWarningText;
    public Button FinalDeleteButton;
    public Button CancelButton;
    public Text CampaignNameText;
    public Button SwitchButton;
    public Button ReorderButton;
    public Text InstructionText;
    public CampaignOverview CampaignOverview;
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;

    private PointerEventData pointerEventData;
    private bool isInteractingWithUI = false;

    void Start () {
        InstructionText.text = Instruction1;

        if (Global.CurrentCampaign == "")
        {
            CampaignNameText.text = "New Campaign";
        }
        else
        {
            CampaignNameText.text = Global.CurrentCampaign;
        }
	}

    public void Edit()
    {
        // if selected level has 2 numbers divided by comma, go to new level creation mode (not load)
        // when saving use these 2 numbers in the beginning of the file.
        // get to a point where you can try the flow of the game -> creation mode -> new campaign -> campaign overview -> edit mode ->
        // campaign overview -> (exit) -> main menu -> ...all saved properly...
        
        if (CampaignOverview.GetSelectedLevelName() != "")
        {
            Global.CurrentLevel = CampaignOverview.GetSelectedLevelName();
            SceneManager.LoadScene("CreationMode");
        }
    }

    public void Delete()
    {
        EditButton.gameObject.SetActive(false);
        DeleteButton.gameObject.SetActive(false);
        DeleteWarningText.gameObject.SetActive(true);
        FinalDeleteButton.gameObject.SetActive(true);
        CancelButton.gameObject.SetActive(true);
    }

    public void FinalDelete()
    {
        CampaignOverview.DeleteSelectedTile();
        Cancel();
    }

    public void Switch()
    {
        CampaignOverview.Switch();
    }

    // in playmode level selection chooses the first level that it find which have the same order.
    public void Reorder()
    {
        if (GetIsReorderActive())
        {
            ReorderButton.GetComponent<Image>().color = Color.white;
            InstructionText.text = Instruction1;

            CampaignOverview.EndReordering();
        }
        else
        {
            ReorderButton.GetComponent<Image>().color = Const.SelectedColor;
            InstructionText.text = Instruction2;

            HideUI();
            CampaignOverview.StartReordering();
        }
    }

    public void HideUI()
    {
        EditButton.gameObject.SetActive(false);
        DeleteButton.gameObject.SetActive(false);
        SwitchButton.gameObject.SetActive(false);
        DeleteWarningText.gameObject.SetActive(false);
        FinalDeleteButton.gameObject.SetActive(false);
        CancelButton.gameObject.SetActive(false);
    }

    public void ShowReorder()
    {
        ReorderButton.gameObject.SetActive(true);
    }

    public void ShowEdit()
    {
        EditButton.gameObject.SetActive(true);
    }

    public void ShowDelete()
    {
        DeleteButton.gameObject.SetActive(true);
    }

    public void ShowSwitch()
    {
        SwitchButton.gameObject.SetActive(true);
    }

    public void Cancel()
    {
        EditButton.gameObject.SetActive(true);
        DeleteButton.gameObject.SetActive(true);
        DeleteWarningText.gameObject.SetActive(false);
        FinalDeleteButton.gameObject.SetActive(false);
        CancelButton.gameObject.SetActive(false);
    }

    public void ExitToMainMenu()
    {
        Global.CurrentCampaign = "";
        Global.CurrentLevel = "";
        SceneManager.LoadScene("MainMenu");
    }
	
	void Update () {
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

    public bool GetIsReorderActive()
    {
        return ReorderButton.GetComponent<Image>().color == Const.SelectedColor;
    }

    public void DeselectReorderButton()
    {
        ReorderButton.GetComponent<Image>().color = Color.white;
    }
}
