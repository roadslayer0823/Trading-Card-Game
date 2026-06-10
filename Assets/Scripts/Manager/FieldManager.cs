using UnityEngine;

public class FieldManager : MonoBehaviour
{
    public Transform fieldParent;
    public GameObject slotPrefab;
    public int slotCount = 4;

    private void Start()
    {
        for (int i = 0; i < slotCount; i++) 
        {
            GameObject slotObj = Instantiate(slotPrefab, fieldParent);
            slotObj.name = "FieldSlot_" + (i + 1);

            FieldSlot slot = slotObj.GetComponent<FieldSlot>();
            if(slot != null)
            {
                slot.slotIndex = i;
                Debug.Log($"Slot {slotObj.name} 索引設為 {i}");
            }
        }
    }
}
