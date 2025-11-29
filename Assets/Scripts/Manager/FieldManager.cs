using System.Collections;
using System.Collections.Generic;
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
            GameObject slot = Instantiate(slotPrefab, fieldParent);
            slot.name = "FieldSlot_" + (i + 1);
        }
    }
}
