using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [Header("Objects in the World")]
    [SerializeField] private GameObject hotDog;
    [SerializeField] private GameObject badge;
    [SerializeField] private GameObject sunglasses;
    [SerializeField] private GameObject ball;
    //[SerializeField] private GameObject hat;

    [Header("Ref")]
    DialogueManager dialogueManager;
    InventorySystem inventory;

    // All Objects at the start are all deactivated
    void Start()
    {
        dialogueManager = DialogueManager.Instance;
        inventory = FindFirstObjectByType<InventorySystem>();

        if (hotDog != null) hotDog.SetActive(false);
        if (badge != null) badge.SetActive(false);
        if (sunglasses != null) sunglasses.SetActive(false);
        if (ball != null) ball.SetActive(false);
    }

    void Update()
    {
        if (dialogueManager == null || inventory == null) return;
        
        ShowWhenQuestActive(ball, "ballQuest", inventory.ballAquired);
        ShowWhenQuestActive(hotDog, "hotdogQuest", inventory.hotdogAquired);
        ShowWhenQuestActive(sunglasses, "sunglassesQuest", inventory.sunglassesAquired);
        ShowWhenQuestActive(badge, "badgeQuest", inventory.badgeAquired);
    }

    void ShowWhenQuestActive(GameObject item, string questVar, bool alreadyCollected)
    {
        if(item == null) return;
        if(alreadyCollected) return;
        if(item.activeSelf) return;

        if(dialogueManager.GetInkBool(questVar))
            item.SetActive(true);
    }
}
