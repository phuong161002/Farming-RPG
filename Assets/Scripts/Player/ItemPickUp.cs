using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Item item = collision.GetComponent<Item>();
        if(item != null)
        {
            // Get item detail
            ItemDetail itemDetail = InventoryManager.Instance.GetItemDetail(item.ItemCode);
            if(itemDetail != null)
            {
                if(itemDetail.canBePickedUp)
                {
                    InventoryManager.Instance.AddItem(InventoryLocation.Player, item, collision.gameObject);
                }
            }
        }
    }
}
