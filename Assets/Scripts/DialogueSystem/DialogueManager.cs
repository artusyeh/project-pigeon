using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Ink.Runtime;
using TMPro;
using System.Security.AccessControl;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    Story story;
    TextArchitect architect;
    
    [Header("Ink File")]
    private TextAsset inkFile;

    [Header("Choice UI")]
    public Transform choicePanel;
    public GameObject choiceButtonPrefab;

    public CanvasGroup dialogueRoot;

    [Header("Ink Variables")]
    public string char_choice;

    [Header("Additional States")]
    public bool IsDialogueActive = false;

    [Header("Ref")]
    private InventorySystem inventorySystem;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Debug.Log("inkFile: " + inkFile);
        // Debug.Log("DialogueSystem.instance: " + DialogueSystem.instance);

        // if (DialogueSystem.instance != null)
        // {
        //     Debug.Log("dialogueContainer: " + DialogueSystem.instance.dialogueContainer);

        //     if (DialogueSystem.instance.dialogueContainer != null)
        //     {
        //         Debug.Log("dialogueText: " + DialogueSystem.instance.dialogueContainer.dialogueText);
        //     }
        // }

        // Connect Architect to your DialogueSystem text
        architect = new TextArchitect(DialogueSystem.instance.dialogueContainer.dialogueText);

        if(dialogueRoot != null)
            dialogueRoot.gameObject.SetActive(false);
        if(choicePanel != null)
            choicePanel.gameObject.SetActive(false);
    }

    void Update()
    {
        if(!IsDialogueActive || story == null) return;

        if(Input.GetKeyDown(KeyCode.Space))
        {
            //If text is still typing
            if(architect.isBuilding)
            {
                if(!architect.hurryUp)
                {
                    architect.hurryUp = true;
                }
                else
                {
                    architect.ForceComplete();
                }

                return;
            }

            AdvanceStory();
        }
    }

    public void StartDialogue(TextAsset newInkFile, InventorySystem inventory = null)
    {
        if (newInkFile == null || IsDialogueActive) return;

        IsDialogueActive = true;
        inkFile = newInkFile;
        story = new Story(inkFile.text);

        //Sync the player's inventory booleans into Ink variables
        if (inventory != null) 
        {
            SyncInventoryToStory(inventory);
        }
        
        if (dialogueRoot != null)
            dialogueRoot.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowFirstLine();
    }

    private void SyncInventoryToStory(InventorySystem inventory)
    {
        void SetBool(string varName, bool val)
        {
            if(story.variablesState.GlobalVariableExistsWithName(varName))
                story.variablesState[varName] = val;
        }

        // Match these strings to the exact VAR names at the top of your .ink files
        SetBool("hotdogAquired", inventory.hotdogAquired);
        SetBool("badgeAquired", inventory.badgeAquired);
        SetBool("sunglassesAquired", inventory.sunglassesAquired);
        SetBool("ballAquired", inventory.ballAquired);
        SetBool("hatEquipped", inventory.hatEquipped);
        SetBool("hatRecieved", inventory.hatRecieved);
    }

    public void AdvanceStory()
    {
        // If Ink has more text
        if (story.canContinue)
        {
            string line = story.Continue();

            ApplyTags();
            architect.Build(line);
            return;
        }

        // If no more text but there are choices
        if (story.currentChoices.Count > 0)
        {
            ShowChoices();
            return;
        }

        //End of Story
        EndDialogue();

        Debug.Log("END OF STORY " + SceneManager.GetActiveScene().name);
        //SceneManagerScript sm = SceneManagerScript.Instance;
        //Debug.Log("Calling Scene Manager should be done?");
        //sm.SwitchSceneTime(SceneManager.GetActiveScene().name);
    }

    void ShowFirstLine()
    {
        if(story.canContinue)
        {
            string line = story.Continue();
            ApplyTags();
            architect.Build(line);
        }
    }

    //Allows user to use tags for characters, expressions, etc on Inky
    void ApplyTags()
    {
        foreach (string tag in story.currentTags)
        {
            string[] split = tag.Split(' ');

            if (split.Length < 2)
                continue;

            string prefix = split[0].ToLower();
            string param = string.Join(" ", split, 1, split.Length - 1);
            // string.Join(string separator, string[] array, int startIndex, int count)
            // join the words together when there is a space, then start taking elements from index 1 until the last element (which is split.Length - 1)

            switch (prefix)
            {
                case "speaker":
                    SetSpeaker(param);
                    break;
                
                //Add more tags here
            }
        }
    }

    public void EndDialogue()
    {
        IsDialogueActive = false;
        story = null;

        if(dialogueRoot != null)
            dialogueRoot.gameObject.SetActive(false);
        
        if(choicePanel != null)
            choicePanel.gameObject.SetActive(false);
        
        //Re-Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ---- Character ----
    Character activeCharacter;

    void SetSpeaker(string speakerName)
    {
        activeCharacter = CharacterManager.Instance.GetCharacter(speakerName);

        if (activeCharacter != null)
        {
            DialogueSystem.instance.dialogueContainer.nameText.text = activeCharacter.characterName;

            //New Part: Activate the game object of the current speaker
            CharacterManager.Instance.SetActiveSpeaker(activeCharacter);
        }
        else
        {
            Debug.LogWarning("No character found: " + speakerName);
        }
    }

    //Set Expression Function
    void SetExpression(string expression)
    {
        if (activeCharacter != null)
            activeCharacter.SetExpression(expression);
    }

    // ---- Choices ---- //
    void ShowChoices()
    {
        choicePanel.gameObject.SetActive(true);

        // Deactivate the Textboxes
        dialogueRoot.gameObject.SetActive(false);

        // Clear old buttons
        foreach (Transform child in choicePanel)
            Destroy(child.gameObject);

        // Troubleshooting this section
        // Create a button for each choice
        for (int i = 0; i < story.currentChoices.Count; i++)
        {
            Choice c = story.currentChoices[i];
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicePanel);
            ChoiceButton btn = buttonObj.GetComponent<ChoiceButton>();
            btn.Init(c.text, i, OnChoiceSelected); //spawn button
        }
    }

    void OnChoiceSelected(int choiceIndex)
    {
        choicePanel.gameObject.SetActive(false);
        dialogueRoot.gameObject.SetActive(true);

        story.ChooseChoiceIndex(choiceIndex);

        AdvanceStory();
    }
}
