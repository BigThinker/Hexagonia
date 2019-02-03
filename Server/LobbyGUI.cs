using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyGUI : MonoBehaviour
{
    public Button HostButton;
    public Text Player1Name;
    public Text Player2Name;
    public Text Player3Name;
    public Image Player1Color;
    public Image Player2Color;
    public Image Player3Color;
    public Button StartGameButton;
    public Multiplayer Lobby;

    public void Host()
    {
        if (Lobby.Host())
        {
            ShowUI();
        }
    }

    public void ShowUI()
    {
        HostButton.gameObject.SetActive(false);
        Player1Name.gameObject.SetActive(true);
        Player2Name.gameObject.SetActive(true);
        Player3Name.gameObject.SetActive(true);
        Player1Color.gameObject.SetActive(true);
        Player2Color.gameObject.SetActive(true);
        Player3Color.gameObject.SetActive(true);
        StartGameButton.gameObject.SetActive(true);
    }

    public void StartGame()
    {
        if (Global.CurrentCampaign != "")
        {
            Lobby.StartGame();
        }
        else
        {
            Debug.Log("You forgot to set Global.CurrentCampaign.");
        }
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

        foreach(var entry in Lobby.Players)
        {
            Player player = entry.Value;

            if (Player1Name.text == "")
            {
                Player1Name.text = player.Name;
                Player1Color.color = player.Color;
            }
            else if (Player2Name.text == "")
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
