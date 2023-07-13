using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Queue<GameObject>[] VIP = new Queue<GameObject>[3];
    public LinkedList<GameObject>[] DP = new LinkedList<GameObject>[3];
    public static int elevatorOnFloor;

    private void Start()
    {
        elevatorOnFloor = 0;
        for(int i = 0; i < VIP.Length; i++)
        {
            VIP[i] = new Queue<GameObject>();
            DP[i] = new LinkedList<GameObject>();
        }
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.T))
        {
            Debug.Log("Pressed");
            PrintList();
        }
    }

    private void PrintList()
    {
        foreach(var item in DP)
        {
            Debug.Log("Hi " + item);
        }
    }    
}
