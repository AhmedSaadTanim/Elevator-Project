using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorQueue : MonoBehaviour
{
    readonly public static string childName = "FloorPassangers";
    void Start()
    {
       GameObject floorPassangers = new(childName);
       Instantiate(floorPassangers, transform);
       Destroy(floorPassangers);
    }
}
