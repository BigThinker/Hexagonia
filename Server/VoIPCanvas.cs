using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VoIPCanvas : MonoBehaviour
{
    public static bool IsVoiceToggleActive = false;

    private static VoIPCanvas instance;

    public Toggle VoiceToggle;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Update()
    {

    }

    public void OnVoiceToggleChanged()
    {
        IsVoiceToggleActive = VoiceToggle.isOn;
    }
}
