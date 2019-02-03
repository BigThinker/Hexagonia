using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyGUI : MonoBehaviour
{
    public InputField IPField;
    public Button JoinButton;
    public Text Player1Name;
    public Text Player2Name;
    public Text Player3Name;
    public Image Player1Color;
    public Image Player2Color;
    public Image Player3Color;
    public Text Waiting;

    Multiplayer Lobby;
    GraphicRaycaster raycaster;
    PointerEventData pointerEventData;
    EventSystem eventSystem;

    void Start()
    {
        Lobby = FindObjectOfType<Multiplayer>();
        raycaster = FindObjectOfType<GraphicRaycaster>();
        eventSystem = FindObjectOfType<EventSystem>();
    }
    
    public void Join()
    {
        Lobby.Join();
    }

    public void ShowUI()
    {
        IPField.gameObject.SetActive(false);
        JoinButton.gameObject.SetActive(false);
        Player1Name.gameObject.SetActive(true);
        Player2Name.gameObject.SetActive(true);
        Player3Name.gameObject.SetActive(true);
        Player1Color.gameObject.SetActive(true);
        Player2Color.gameObject.SetActive(true);
        Player3Color.gameObject.SetActive(true);
        Waiting.gameObject.SetActive(true);
    }

    public void HideUI()
    {
        IPField.gameObject.SetActive(true);
        JoinButton.gameObject.SetActive(true);
        Player1Name.gameObject.SetActive(false);
        Player2Name.gameObject.SetActive(false);
        Player3Name.gameObject.SetActive(false);
        Player1Color.gameObject.SetActive(false);
        Player2Color.gameObject.SetActive(false);
        Player3Color.gameObject.SetActive(false);
        Waiting.gameObject.SetActive(false);
    }

    public string GetIPAddress()
    {
        if (IPField.text == "")
        {
            // 127.0.0.1
            return IPField.placeholder.GetComponent<Text>().text;
        }

        return IPField.text;
    }

    public void ClearUI()
    {
        Player1Name.text = "";
        Player1Color.color = Color.white;
        Player2Name.text = "";
        Player2Color.color = Color.white;
        Player3Name.text = "";
        Player3Color.color = Color.white;
    }

    public void UpdateUI()
    {
        ClearUI();

        if (Lobby.LocalPlayer != null)
        {
            Player1Name.text = Lobby.LocalPlayer.Name + " (me)";
            Player1Color.color = Lobby.LocalPlayer.Color;
        }

        foreach(var entry in Lobby.Players)
        {
            Player player = entry.Value;

            if (player != Lobby.LocalPlayer)
            {
                if (Player2Name.text == "")
                {
                    Player2Name.text = player.Name;
                    Player2Color.color = player.Color;
                }
                else if (Player3Name.text == "")
                {
                    Player3Name.text = player.Name;
                    Player3Color.color = player.Color;
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;
            
            List<RaycastResult> results = new List<RaycastResult>();
            
            raycaster.Raycast(pointerEventData, results);
            
            foreach (RaycastResult result in results)
            {
                switch(result.gameObject.name)
                {
                    case "Join":
                        Join();
                        break;
                }
            }
        }
    }
}
