using UnityEngine;

public enum ItemType
{
    Door,
    Hax,
    Bed,
    BadHax,
    Medicine,
    Tree,
    House,
    Shop
}

public class Item : MonoBehaviour
{
    public ItemType type;

    bool insideInventory;

    public void OnInventoryAdd()
    {
        Destroy(GetComponent<Rigidbody>());

        insideInventory = true;
    }

    public void OnInventoryRemove()
    {
        gameObject.AddComponent<Rigidbody>();
        var rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        insideInventory = false;
    }
}
