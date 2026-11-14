using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceButton : MonoBehaviour
{
    public TextMeshProUGUI buttonText;
    public Button button;

    private int choiceIndex;
    private Action<int> onSelectCallback;

    public void Init(string text, int index, Action<int> callback)
    {
        if (buttonText != null)
        {
            buttonText.text = text;
        }

        choiceIndex = index;
        onSelectCallback = callback;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        onSelectCallback?.Invoke(choiceIndex);
    }
}