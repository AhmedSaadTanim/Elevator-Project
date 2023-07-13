using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    ElevatorSystem elevatorSystem;
    void Start()
    {
        elevatorSystem = GameObject.Find("elevatorDisplay").GetComponent<ElevatorSystem>();
    }

    public void OnAnimationEnds()
    {
        elevatorSystem.UnLoadPassangers();
    }
}
