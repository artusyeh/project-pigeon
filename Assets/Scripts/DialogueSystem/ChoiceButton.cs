using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ChoiceButton : MonoBehaviour
{
    public TextMeshProUGUI buttonText;
    public Button button;

    private int choiceIndex;
    private Action<int> onSelectCallback;

    public void Init(string text, int index, Action<int> callback)
    {
        Debug.Log($"Init called. button assigned: {button != null}, callback null: {callback == null}", this);

        if (buttonText != null)
        {
            buttonText.text = text;
        }

        choiceIndex = index;
        onSelectCallback = callback;

        if (button != null)
        {
            button.onClick.AddListener(() => Debug.Log("onClick lambda fired"));
            button.onClick.AddListener(HandleClick);
        }
        else
        {
            Debug.LogError("ChoiceButton: 'button' field is NOT assigned!", this);
        }
    }

    private void HandleClick()
    {
        Debug.Log("HandleClick fired");
        onSelectCallback?.Invoke(choiceIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("PointerClick reached: " + eventData.pointerPress, this);
    }
}