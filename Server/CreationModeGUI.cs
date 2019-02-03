using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreationModeGUI : MonoBehaviour {

    public InputField CampaignNameField;
    public InputField LevelNameField;
    public Button FinalSaveButton;
    public Button CancelButton;
    public Button SaveButton;
    public Button DoneButton;
    public Text DoneMessage;
    public Button ExitButton;
    public Text BrushSizeText;
    public Slider BrushSizeSlider;
    public Slider MapSizeSlider;
    public Button StartButton;
    public Text EnvironmentText;
    public Button ElevationButton;
    public Dropdown MaterialSelectionDropdown;
    public Button BrushEraseButton;
    public Text GameObjectsText;
    public Dropdown GeneralObjectsDropdown;
    public Dropdown GameplayObjectsDropdown;
    public CreationModeEditor Editor;

    private int levelTileX;
    private int levelTileY;
    private int levelOrder;

    GraphicRaycaster[] raycasters;
    EventSystem eventSystem;
    PointerEventData pointerEventData;
    bool isInteractingWithUI = false;

    private void Start()
    {
        // Global.CurrentCampaign = "Campaign2";
        // Global.CurrentLevel = "Level1";

        eventSystem = FindObjectOfType<EventSystem>();

        AddMaterialsOptions();
        AddGeneralObjectsOptions();
        AddGameplayObjectsOptions();

        CheckSelectedLevel();
    }

    void AddMaterialsOptions()
    {
        MaterialSelectionDropdown.ClearOptions();
        MaterialSelectionDropdown.AddOptions(Editor.Materials.Select(x => x.name).ToList());
    }

    void AddGeneralObjectsOptions()
    {
        GeneralObjectsDropdown.ClearOptions();
    }

    void AddGameplayObjectsOptions()
    {
        GameplayObjectsDropdown.ClearOptions();
        List<string> gameplayObjectsNames = new List<string>();

        for (int i = Const.GameplayObjectStartIndex; i < Editor.Objects.Length; i++)
        {
            gameplayObjectsNames.Add(Editor.Objects[i].name);
        }

        GameplayObjectsDropdown.AddOptions(gameplayObjectsNames);
    }

    void CheckSelectedLevel()
    {
        // it's a new level (plus hexagon was selected in campaign overview).
        if (Const.ParseLevelName(Global.CurrentLevel, out levelTileX, out levelTileY) 
            || Global.CurrentLevel == "")
        {
            MapSizeSlider.gameObject.SetActive(true);
            StartButton.gameObject.SetActive(true);
            Editor.OnSizeChanged(MapSizeSlider.value);

            string campaignName;
            // in case of creation mode -> new campaign...there is not yet a defined campaign so autogenerate a name for saving.
            if (Global.CurrentCampaign == "")
            {
                campaignName = "Campaign" + Random.Range(0, 10000);
            }
            else
            {
                campaignName = Global.CurrentCampaign;
            }
            CampaignNameField.placeholder.GetComponent<Text>().text = campaignName;
            LevelNameField.placeholder.GetComponent<Text>().text = "Level" + Random.Range(0, 10000);
            levelOrder = Global.CurrentLevelOrder;
        }
        else
        {
            CampaignNameField.placeholder.GetComponent<Text>().text = Global.CurrentCampaign;
            LevelNameField.placeholder.GetComponent<Text>().text = Global.CurrentLevel;
            TryLoadLevel();
        }
    }

    void DeselectEverything()
    {
        ElevationButton.GetComponent<Image>().color = Color.white;
        MaterialSelectionDropdown.GetComponent<Image>().color = Color.white;
        BrushEraseButton.GetComponent<Image>().color = Color.white;
        GeneralObjectsDropdown.GetComponent<Image>().color = Color.white;
        GameplayObjectsDropdown.GetComponent<Image>().color = Color.white;
    }

    public bool GetIsInteractingWithUI()
    {
        return isInteractingWithUI;
    }

    private void Update()
    {
        raycasters = FindObjectsOfType<GraphicRaycaster>();
        isInteractingWithUI = false;

        foreach (var raycaster in raycasters)
        {
            pointerEventData = new PointerEventData(eventSystem);

            //Set the Pointer Event Position to that of the mouse position
            pointerEventData.position = Input.mousePosition;

            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            raycaster.Raycast(pointerEventData, results);

            // ignore blocker since it fills up a lot of the viewable area.
            results = results.Where(x => x.gameObject.name != "Blocker").ToList();

            if (!isInteractingWithUI)
            {
                isInteractingWithUI = results.Count > 0;
            }

            if (Input.GetMouseButtonDown(0))
            {
                foreach (RaycastResult result in results)
                {
                    switch (result.gameObject.name)
                    {
                        case "Material":
                            DeselectEverything();
                            result.gameObject.GetComponent<Image>().color = Const.SelectedColor;
                            break;
                        case "GeneralObjects":
                            DeselectEverything();
                            result.gameObject.GetComponent<Image>().color = Const.SelectedColor;
                            ChangeBrushSize();
                            break;
                        case "GameplayObjects":
                            DeselectEverything();
                            result.gameObject.GetComponent<Image>().color = Const.SelectedColor;
                            ChangeBrushSize();
                            break;
                    }
                }
            }
        }
    }

    public void StartEditing()
    {
        StartButton.gameObject.SetActive(false);
        MapSizeSlider.gameObject.SetActive(false);
        ShowEditingUI();
    }

    void ShowEditingUI()
    {
        SaveButton.gameObject.SetActive(true);
        DoneButton.gameObject.SetActive(true);
        BrushSizeText.gameObject.SetActive(true);
        BrushSizeSlider.gameObject.SetActive(true);
        EnvironmentText.gameObject.SetActive(true);
        ElevationButton.gameObject.SetActive(true);
        MaterialSelectionDropdown.gameObject.SetActive(true);
        BrushEraseButton.gameObject.SetActive(true);
        GameObjectsText.gameObject.SetActive(true);
        GeneralObjectsDropdown.gameObject.SetActive(true);
        GameplayObjectsDropdown.gameObject.SetActive(true);
    }

    public void ChangeBrushSize()
    {
        // can only put 1 general or gameplay object at a time.
        if (GetSelectedGeneralObject() > -1 || GetSelectedGameplayObject() > -1)
        {
            BrushSizeSlider.value = 0;
        }

        Editor.SetRange(BrushSizeSlider.value);
        BrushSizeText.text = "Brush size: " + Editor.GetRange();
    }

    public void Elevate()
    {
        if (GetIsElevationSelected())
        {
            DeselectEverything();
        }
        else
        {
            DeselectEverything();
            ElevationButton.GetComponent<Image>().color = Const.SelectedColor;
        }
    }

    public void Erase()
    {
        if (GetIsEraseSelected())
        {
            DeselectEverything();
        }
        else
        {
            DeselectEverything();
            BrushEraseButton.GetComponent<Image>().color = Const.SelectedColor;
        }
    }

    public bool GetIsElevationSelected()
    {
        return ElevationButton.GetComponent<Image>().color == Const.SelectedColor;
    }

    public bool GetIsEraseSelected()
    {
        return BrushEraseButton.GetComponent<Image>().color == Const.SelectedColor;
    }

    public int GetSelectedMaterial()
    {
        if (MaterialSelectionDropdown.GetComponent<Image>().color == Color.white)
        {
            return -1;
        }
        else
        {
            return MaterialSelectionDropdown.value;
        }
    }

    public int GetSelectedGeneralObject()
    {
        if (GeneralObjectsDropdown.GetComponent<Image>().color == Color.white)
        {
            return -1;
        }
        else
        {
            return GeneralObjectsDropdown.value;
        }
    }

    public int GetSelectedGameplayObject()
    {
        if (GameplayObjectsDropdown.GetComponent<Image>().color == Color.white)
        {
            return -1;
        }
        else
        {
            return GameplayObjectsDropdown.value;
        }
    }

    public string GetCampaignName()
    {
        return CampaignNameField.text != "" ? CampaignNameField.text : CampaignNameField.placeholder.GetComponent<Text>().text;
    }

    public string GetLevelName()
    {
        return LevelNameField.text != "" ? LevelNameField.text : LevelNameField.placeholder.GetComponent<Text>().text;
    }

    public void Save()
    {
        SaveButton.gameObject.SetActive(false);
        DoneButton.gameObject.SetActive(false);
        // in case we came from creation mode -> new campaign -> show campaign name field.
        if (Global.CurrentCampaign == "")
        {
            CampaignNameField.gameObject.SetActive(true);
        }
        int tileX, tileY;
        // in case we came from creation mode -> load/new campaign -> campaign overview -> plus hexagon -> set level name field to true.
        if (Const.ParseLevelName(Global.CurrentLevel, out tileX, out tileY) 
            || Global.CurrentLevel == "")
        {
            LevelNameField.gameObject.SetActive(true);
        }
        CancelButton.gameObject.SetActive(true);
        FinalSaveButton.gameObject.SetActive(true);
    }

    public void Done()
    {
        SaveButton.gameObject.SetActive(false);
        DoneButton.gameObject.SetActive(false);
        DoneMessage.gameObject.SetActive(true);
        CancelButton.gameObject.SetActive(true);
        ExitButton.gameObject.SetActive(true);
    }

    public void Exit()
    {
        SceneManager.LoadScene("CampaignOverview");
    }

    public void Cancel()
    {
        SaveButton.gameObject.SetActive(true);
        DoneButton.gameObject.SetActive(true);
        CampaignNameField.gameObject.SetActive(false);
        LevelNameField.gameObject.SetActive(false);
        CancelButton.gameObject.SetActive(false);
        FinalSaveButton.gameObject.SetActive(false);
        DoneMessage.gameObject.SetActive(false);
        ExitButton.gameObject.SetActive(false);
    }

    public void FinalSave()
    {
        // make sure directory exists before accessing it.
        DirectoryInfo dirInfo;
        if (Application.isEditor)
        {
            dirInfo = Directory.CreateDirectory(Application.dataPath + "/Campaigns/Offline/" + GetCampaignName());
        }
        else
        {
            dirInfo = Directory.CreateDirectory(Application.persistentDataPath + "/Campaigns/Offline/" + GetCampaignName());
        }

        TextWriter tw;
        if (Application.isEditor)
        {
            tw = new StreamWriter(Application.dataPath + "/Campaigns/Offline/" + GetCampaignName() + "/" + GetLevelName() + Const.LevelFileExtension);
        }
        else
        {
            tw = new StreamWriter(Application.persistentDataPath + "/Campaigns/Offline/" + GetCampaignName() + "/" + GetLevelName() + Const.LevelFileExtension);
        }

        Dictionary<string, List<GameObject>> grid = Editor.GetGrid();
        tw.Write(grid.Count + "\n");
        tw.Write(levelOrder + "\n");
        tw.Write(levelTileX + "," + levelTileY + "\n");
        foreach (var entry in grid)
        {
            List<GameObject> gameObjects = entry.Value;
            Tile tile = gameObjects[0].GetComponent<Tile>();

            int material = -1;
            string materialName = Const.GetMeshFromTile(tile).material.name;
            if (materialName.Contains(Editor.Materials[0].name))
            {
                material = 0;
            }
            else if (materialName.Contains(Editor.Materials[1].name))
            {
                material = 1;
            }
            else if (materialName.Contains(Editor.Materials[2].name))
            {
                material = 2;
            }

            tw.Write(tile.index.x + "," +
                    tile.index.z + "," + 
                    tile.transform.localScale.y + "," +
                    material);

            if (gameObjects.Count > 1)
            {
                tw.Write(",");
            }

            for (int i = 1; i < gameObjects.Count; i++)
            {
                GameObject go = gameObjects[i];
                if (go != null)
                {
                    int objectId = -1;
                    if (go.name.Contains("BigTree")) {
                        objectId = 0;
                    }
                    else if (go.name.Contains("PineTree")) {
                        objectId = 1;
                    }
                    else if (go.name.Contains("Rock01")) {
                        objectId = 2;
                    }
                    else if (go.name.Contains("Rock02")) {
                        objectId = 3;
                    }
                    else if (go.name.Contains("SpawnPoint")) {
                        objectId = 4;
                    }
                    else if (go.name.Contains("GameMaster")) {
                        objectId = 5;
                    }

                    Vector3 position = go.transform.position;
                    float rotationY = go.transform.rotation.eulerAngles.y;
                    float scale = go.transform.localScale.x;

                    tw.Write(objectId + ","
                            + position.x + "," + position.y + "," + position.z + ","
                            + rotationY + ","
                            + scale);

                    if (i < gameObjects.Count - 1)
                    {
                        tw.Write(",");
                    }
                }
            }
            tw.Write("\n");
        }

        tw.Flush();
        tw.Close();

        Cancel();

        // in creation mode -> new campaign -> once we save we set the current campaign to the newly created folder.
        if (Global.CurrentCampaign == "")
        {
            Global.CurrentCampaign = GetCampaignName();
        }
    }

    public bool TryLoadLevel()
    {
        TextReader tr;
        if (Application.isEditor)
        {
            tr = new StreamReader(Application.dataPath + "/Campaigns/Offline/"
                                + Global.CurrentCampaign + "/"
                                + Global.CurrentLevel + Const.LevelFileExtension);
        }
        else
        {
            tr = new StreamReader(Application.persistentDataPath + "/Campaigns/Offline/"
                                + Global.CurrentCampaign + "/"
                                + Global.CurrentLevel + Const.LevelFileExtension);
        }

        string line;
        int lineNumber = 0;
        while ((line = tr.ReadLine()) != null)
        {
            string[] items = line.Split(',');
            if (lineNumber > 2)
            {
                // parse tile
                int tileX = int.Parse(items[0]);
                int tileZ = int.Parse(items[1]);
                float tileScaleY = float.Parse(items[2]);
                int tileMaterial = int.Parse(items[3]);

                LoadTile loadTile = new LoadTile(tileX, tileZ, tileScaleY, tileMaterial);

                // parse objects
                int numObjects = (items.Length - 4) / 6;
                for (int i = 0; i < numObjects; i++)
                {
                    int start = 4 + i * 6;
                    int objectId = int.Parse(items[start]);
                    Vector3 position = new Vector3(float.Parse(items[start + 1]),
                                                    float.Parse(items[start + 2]),
                                                    float.Parse(items[start + 3]));
                    float rotationY = float.Parse(items[start + 4]);
                    float scale = float.Parse(items[start + 5]);

                    loadTile.AddGameObject(objectId, position, rotationY, scale);
                }

                Editor.QueueTileToLoad(loadTile);
            }
            else if (lineNumber == 1)
            {
                levelOrder = int.Parse(items[0]);
            }
            else if (lineNumber == 2)
            {
                levelTileX = int.Parse(items[0]);
                levelTileY = int.Parse(items[1]);
            }

            lineNumber++;
        }

        Editor.LoadLevel();

        ShowEditingUI();

        return false;
    }

    public void OnMapSizeChanged()
    {
        Editor.OnSizeChanged(MapSizeSlider.value);
    }
}
