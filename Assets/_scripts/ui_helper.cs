using System.Collections;
using TMPro;
using UnityEngine;


public class ui_helper : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI PlayerPromptTextField;
    public void UpdatePromptUi(string GOname)
    {
        if (PlayerPromptTextField != null)
            PlayerPromptTextField.text = $"{GOname}";
    }
  
}
