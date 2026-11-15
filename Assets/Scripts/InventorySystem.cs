using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{

    //items the pigeon must retrieve
    public bool hotdogAquired;
    public bool badgeAquired;
    public bool sunglassesAquired;
    public bool ballAquired;

    //if boy gives you the hat, set this to true
    public bool hatEquipped;
    public bool hatRecieved;


    public List<GameObject> Inventory;
    public Transform hatSpawnPoint;
    public GameObject hatPrefab;
    public GameObject pigeon;
    
    

    // Update is called once per frame
    void Update()
    {
        if (!hatEquipped && hatRecieved)
        {
            GameObject myHat = Instantiate(hatPrefab, hatSpawnPoint.position, hatPrefab.transform.rotation);
            //parent hat to pigeon
            myHat.transform.parent = pigeon.transform;
            hatEquipped = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Item"))
        {
            Inventory.Add(other.gameObject);
            Destroy(other.gameObject);
            if (other.gameObject.name == "ball")
            {
                ballAquired = true;
            }
            else if (other.gameObject.name == "glasses")
            {
                sunglassesAquired = true;
            }
            else if (other.gameObject.name == "badge")
            {
                badgeAquired = true;
            }
            else if (other.gameObject.name == "bun")
            {
                hotdogAquired = true;
            }
        }
    }
}
