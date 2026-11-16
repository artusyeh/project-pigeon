using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Ink.Runtime;

public class Character : MonoBehaviour
{
    [Header("Ink")]
    [SerializeField] private TextAsset inkFile;

    [Header("Ref")]
    private DialogueManager dialogueManager;

    // -- Ref -- //
    // Items the pigeon must retrieve
    // public bool hotdogAquired;
    // public bool badgeAquired;
    // public bool sunglassesAquired;
    // public bool ballAquired;

    // If boy gives you the hat, set this to true
    // public bool hatEquipped;
    // public bool hatRecieved;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 4.5f;
    //Press E in World Space Icon
    [SerializeField] private GameObject interactPromptUI;
    private Transform playerTransform;
    private bool isPlayerInRange;
    
    [Header("Character")]
    public string characterName;

    [Header("VN Stuff")]

    //Default Portrait for Character
    public Image portrait;

    //Default Expression
    public Sprite defaultPortrait;

    //Sprite Renderer
    public SpriteRenderer spriteRenderer;

    //Dictionary of Expressions + List to Enter them through Inspector
    public List<ExpressionEntry> expressionList = new List<ExpressionEntry>();
    private Dictionary<string, Sprite> expressions = new Dictionary<string, Sprite>();

    //Fade In & Fade Out
    public CanvasGroup canvasGroup;

    public void Awake()
    {
        //Find Bird
        BirdFlightController bird = FindFirstObjectByType<BirdFlightController>();
        if(bird != null)
            playerTransform = bird.transform;
        
        //Interaction Prompt UI
        if(interactPromptUI != null)
            interactPromptUI.SetActive(false);

        //Create Canvas Group
        if(canvasGroup == null)
           canvasGroup = GetComponent<CanvasGroup>();

        //Build dictionary for quick lookup of expressions
        foreach (var entry in expressionList)
            expressions[entry.id.ToLower()] = entry.sprite;

        //Set Default Portrait
        if(portrait != null)
            portrait.sprite = defaultPortrait;
    }

    void Update()
    {
        if(playerTransform == null || inkFile == null) return; 

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        isPlayerInRange = (dist <= interactionRange);

        //If interaction prompt is not null & its not already active
        //Show it on screen
        if(interactPromptUI != null && interactPromptUI.activeSelf != isPlayerInRange){
            interactPromptUI.SetActive(isPlayerInRange);
        }

        if(isPlayerInRange &&
            DialogueManager.Instance != null && 
            !DialogueManager.Instance.IsDialogueActive && 
            Keyboard.current != null && 
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    private void Interact()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError($"[{name}] Cannot interact: No active DialogueManager found in the scene!");
            return;
        }

        if (inkFile == null)
        {
            Debug.LogError($"[{name}] Cannot interact: No inkFile assigned on this Character!");
            return;
        }

        InventorySystem playerInventory = FindFirstObjectByType<InventorySystem>();

        DialogueManager.Instance.StartDialogue(inkFile, playerInventory);
    }

    // -- Existing Functions -- //
    //Dont touch these since it might break the other components
    //Will not be used in this game

    //Play Animations, Switch Expression, etc
    public void SetExpression(string expressionID)
    {
        expressionID = expressionID.ToLower();

        //if expresison is found
        if(expressions.ContainsKey(expressionID))
        {
            portrait.sprite = expressions[expressionID];
            Debug.Log($"{characterName} expression set to {expressionID}");
        }
        else
        {
            Debug.LogWarning($"Expression '{expressionID}' not found for {characterName}");
        }
    }

    [System.Serializable]
    public class ExpressionEntry
    {
        public string id;
        public Sprite sprite;
    }

    //Fade to targetAlpha over duration of time
    public IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float start = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(start, targetAlpha, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    //Helper function to set alpha of a character (Opacity)
    public void SetAlpha(float a)
    {
        if(spriteRenderer != null)
        {
            Color col = spriteRenderer.color;
            col.a = a;
            spriteRenderer.color = col;
        }
    }
}
