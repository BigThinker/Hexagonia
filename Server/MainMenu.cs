using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    enum MainMenuState
    {
        MainMenu,
        CreationMode,
        CreationLoadCampaign,
        PlayMode,
        PlayNewCampaign
    }

    public Button CreationModeButton;
    public Button PlayModeButton;
    public Button NewCampaignButton;
    public Button LoadCampaignButton;
    public Button BackButton;
    public GameObject Scrollview;
    public GameObject ScrollviewContent;
    public GameObject ScrollviewElement;
    public Button LoadButton;

    MainMenuState currentState = MainMenuState.MainMenu;

    GraphicRaycaster raycaster;
    PointerEventData pointerEventData;
    EventSystem eventSystem;

    private void Start()
    {
        raycaster = FindObjectOfType<GraphicRaycaster>();
        eventSystem = FindObjectOfType<EventSystem>();
        
        DirectoryInfo dirInfo;
        if (Application.isEditor)
        {
            dirInfo = Directory.CreateDirectory(Application.dataPath + "/Campaigns/Offline");
            Directory.CreateDirectory(Application.dataPath + "/Campaigns/Online");
        }
        else
        {
            dirInfo = Directory.CreateDirectory(Application.persistentDataPath + "/Campaigns/Offline");
            Directory.CreateDirectory(Application.persistentDataPath + "/Campaigns/Online");
        }

        DirectoryInfo[] directories = dirInfo.GetDirectories();
        Vector2 size = ScrollviewContent.GetComponent<RectTransform>().sizeDelta;
        size.y = directories.Length * 30 + 30;
        ScrollviewContent.GetComponent<RectTransform>().sizeDelta = size;

        for (int i = 0; i < directories.Length; i++)
        {
            GameObject scrollviewElement = Instantiate(ScrollviewElement);
            Vector3 pos = scrollviewElement.GetComponent<RectTransform>().position;
            pos.y = size.y - scrollviewElement.GetComponent<RectTransform>().rect.height + i * -30;
            scrollviewElement.GetComponent<RectTransform>().position = pos;
            scrollviewElement.transform.Find("Text").GetComponent<Text>().text = directories[i].Name;
            scrollviewElement.transform.SetParent(ScrollviewContent.transform, false);
            scrollviewElement.name = "ScrollviewElement:" + directories[i].Name;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pointerEventData = new PointerEventData(eventSystem);
            //Set the Pointer Event Position to that of the mouse position
            pointerEventData.position = Input.mousePosition;

            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            raycaster.Raycast(pointerEventData, results);

            //For every result returned
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.name.StartsWith("ScrollviewElement"))
                {
                    ClearLoadCampaignSelection();

                    result.gameObject.GetComponent<Image>().color = Const.SelectedColor;
                }
            }
        }
    }

    void ClearLoadCampaignSelection()
    {
        foreach (Transform scrollviewElement in ScrollviewContent.transform)
        {
            scrollviewElement.GetComponent<Image>().color = Color.white;
        }
    }

    string GetSelectedCampaignName()
    {
        foreach (Transform scrollviewElement in ScrollviewContent.transform)
        {
            if (scrollviewElement.GetComponent<Image>().color == Const.SelectedColor)
            {
                return scrollviewElement.name.Substring(18, scrollviewElement.name.Length - 18);
            }
        }

        return "";
    }

    public void EnterCreationMode()
    {
        CreationModeButton.gameObject.SetActive(false);
        PlayModeButton.gameObject.SetActive(false);
        NewCampaignButton.gameObject.SetActive(true);
        LoadCampaignButton.gameObject.SetActive(true);
        BackButton.gameObject.SetActive(true);
        currentState = MainMenuState.CreationMode;
    }

    public void EnterPlayMode()
    {
        CreationModeButton.gameObject.SetActive(false);
        PlayModeButton.gameObject.SetActive(false);
        NewCampaignButton.gameObject.SetActive(true);
        LoadCampaignButton.gameObject.SetActive(true);
        BackButton.gameObject.SetActive(true);
        currentState = MainMenuState.PlayMode;
    }

    public void EnterNewCampaign()
    {
        switch (currentState)
        {
            case MainMenuState.CreationMode:
                EnterCreationModeNewCampaign();
                break;
            case MainMenuState.PlayMode:
                EnterPlayModeNewCampaign();
                break;
        }
    }

    void EnterCreationModeNewCampaign()
    {
        SceneManager.LoadScene("CampaignOverview");
    }

    void EnterPlayModeNewCampaign()
    {
        NewCampaignButton.gameObject.SetActive(false);
        LoadCampaignButton.gameObject.SetActive(false);
        Scrollview.gameObject.SetActive(true);
        LoadButton.gameObject.SetActive(true);
        currentState = MainMenuState.PlayNewCampaign;
    }

    public void EnterLoadCampaign()
    {
        switch (currentState)
        {
            case MainMenuState.CreationMode:
                EnterCreationLoadCampaign();
                break;
            case MainMenuState.PlayMode:
                EnterPlayLoadCampaign();
                break;
            default:
                Debug.Log("Something weird happened.");
                break;
        }
    }

    void EnterCreationLoadCampaign()
    {
        NewCampaignButton.gameObject.SetActive(false);
        LoadCampaignButton.gameObject.SetActive(false);
        Scrollview.gameObject.SetActive(true);
        LoadButton.gameObject.SetActive(true);
        currentState = MainMenuState.CreationLoadCampaign;
    }

    public void Load()
    {
        string selectedCampaignName = GetSelectedCampaignName();

        if (selectedCampaignName != "")
        {
            Global.CurrentCampaign = selectedCampaignName;

            switch (currentState)
            {
                case MainMenuState.CreationLoadCampaign:
                    SceneManager.LoadScene("CampaignOverview");
                    break;
                case MainMenuState.PlayNewCampaign:
                    SceneManager.LoadScene("Lobby");
                    break;
            }
        }
    }

    void EnterPlayLoadCampaign()
    {
        // NewCampaignButton.gameObject.SetActive(false);
        // LoadCampaignButton.gameObject.SetActive(false);
    }

    public void Back()
    {
        switch (currentState)
        {
            case MainMenuState.CreationMode:
                BackToMainMenu();
                break;
            case MainMenuState.PlayMode:
                BackToMainMenu();
                break;
            case MainMenuState.CreationLoadCampaign:
                BackToNewAndLoadCampaignMenu();
                currentState = MainMenuState.CreationMode;
                break;
            case MainMenuState.PlayNewCampaign:
                BackToNewAndLoadCampaignMenu();
                currentState = MainMenuState.PlayMode;
                break;
        }
    }

    void BackToMainMenu()
    {
        CreationModeButton.gameObject.SetActive(true);
        PlayModeButton.gameObject.SetActive(true);
        NewCampaignButton.gameObject.SetActive(false);
        LoadCampaignButton.gameObject.SetActive(false);
        BackButton.gameObject.SetActive(false);

        currentState = MainMenuState.MainMenu;
    }

    void BackToNewAndLoadCampaignMenu()
    {
        ClearLoadCampaignSelection();
        Scrollview.gameObject.SetActive(false);
        LoadButton.gameObject.SetActive(false);
        NewCampaignButton.gameObject.SetActive(true);
        LoadCampaignButton.gameObject.SetActive(true);
    }
}
