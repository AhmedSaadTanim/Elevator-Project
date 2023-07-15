using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public LinkedList<GameObject>[] VIP = new LinkedList<GameObject>[3];
    public LinkedList<GameObject>[] DP = new LinkedList<GameObject>[3];
    public static int elevatorOnFloor;

    private void Start()
    {
        elevatorOnFloor = 0;
        for(int i = 0; i < VIP.Length; i++)
        {
            VIP[i] = new LinkedList<GameObject>();
            DP[i] = new LinkedList<GameObject>();
        }
    }
    
    public void SortQueue()
    {
        Debug.Log("Sorting now : " + elevatorOnFloor);
        int index = 0;
        foreach(GameObject p in VIP[elevatorOnFloor])
        {
            p.transform.GetComponent<Passanger>().Move(new Vector2(-(SpawnManager.spawnXStart + index), p.transform.position.y));
            
            index++;
        }

        foreach(GameObject p in DP[elevatorOnFloor])
        {
            p.transform.GetComponent<Passanger>().Move(new Vector2(-(SpawnManager.spawnXStart + index), p.transform.position.y));
            index++;
        }
    }
}
