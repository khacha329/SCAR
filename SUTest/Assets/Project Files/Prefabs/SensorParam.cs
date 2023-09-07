using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SensorParam : MonoBehaviour
{
    public TouchScreenKeyboard keyboard;
    public string DevEUI;
    public bool NameSet = false;
    [SerializeField]
    TextMeshProUGUI NameDisplay;

    public static string keyboardText = "";
    // Start is called before the first frame update
    //void Start()
    //{
    //    keyboard = TouchScreenKeyboard.Open("text to edit", TouchScreenKeyboardType.URL, false, false, false, false);
    //    TouchScreenKeyboard.hideInput = false;
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    if (keyboard.status == TouchScreenKeyboard.Status.Done && !NameSet)
    //    {
    //        keyboardText = keyboard.text;
    //        DevEUI = "a81758fffe" + keyboardText;
    //        DisplayName(DevEUI);
    //        NameSet = true;

    //    }
    //}

    public void DisplayName()
    {
        keyboardText = keyboard.text;
        Debug.Log(keyboard.text);
        DevEUI = "a81758fffe" + keyboardText;
        int StartIndex = 0;
        string ConstructedName = "";
        while (StartIndex < DevEUI.Length)        
        {
                ConstructedName += DevEUI.Substring(StartIndex, 2) + "-";
                StartIndex += 2;           
        }
        NameDisplay.text = ConstructedName.Remove(ConstructedName.Length-1);
    }
}
