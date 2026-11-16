using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Ref DialogueManager to get the chosen Character VAR
    [SerializeField] private DialogueManager dialogueManager;

    private string char_choice;

    void Start()
    {
        CheckChosenCharacter();
    }

    void Update()
    {
        
    }

    void CheckChosenCharacter()
    {
        char_choice = dialogueManager.char_choice;
        Debug.Log("Current Chosen Host: " + char_choice);
    }

    // -- Character -- //
    Character activeCharacter;

    void SetSpeaker()
    {
        activeCharacter = CharacterManager.Instance.GetCharacter(char_choice);

        if (activeCharacter != null)
        {
            //DialogueSystem.instance.dialogueContainer.nameText.text = activeCharacter.characterName;
            //New Part: Activate the game object of the current speaker
            CharacterManager.Instance.SetActiveSpeaker(activeCharacter);
        }
        else
        {
            Debug.LogWarning("No character found: " + char_choice);
        }
    }

}
