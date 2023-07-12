using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElevatorDisplay : MonoBehaviour
{
    public bool hasReached;
    public int to;

    [Header("References")]

    [SerializeField] TextMeshProUGUI floorCounter;
    [SerializeField] TextMeshProUGUI refVipText;
    [SerializeField] TextMeshProUGUI refWeightText;
    [SerializeField] GameObject upArrow;
    [SerializeField] GameObject downArrow;
    [SerializeField] Animator[] elevators;

    [Header("Set Values")]

    [SerializeField] float waitTime;
    [SerializeField] float idleTimer;


    MainManager mainManager;
    Color noColor = new();
    readonly string vipText = "VIP";
    readonly string[] floorNames = { "2", "1", "G" };
    int currentFloor;
    float elapsedTime;
    private void Awake()
    {
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>();
    }
    private void Start()
    {
        SwitchToIdle();
        hasReached = false;
        currentFloor = 0;
        floorCounter.text = floorNames[^(currentFloor + 1)];
    }

    private void SwitchToIdle()
    {
        upArrow.GetComponent<Image>().color = noColor;
        downArrow.GetComponent<Image>().color = noColor;
    }

    private void Update()
    {
        CheckForIdle();
        if (hasReached)
        {
            hasReached = false;
            MoveElevator(currentFloor, to); // to should be decided by checking if passangers are waiting
        }
    }
    private void CheckForIdle()
    {
        if (currentFloor == 0)
        {
            if (Mathf.Abs(elapsedTime - idleTimer) <= 0.05f)
            {
                SwitchToIdle();
            }
            else
            {
                elapsedTime += Time.deltaTime;
            }
        }
        else
        {
            elapsedTime = 0;
        }
    }
    private void MoveElevator(int from, int to)
    {
        int tempValue = from < to ? 1 : -1;
        upArrow.GetComponent<Image>().color = tempValue < 0 ? noColor : Color.white;
        downArrow.GetComponent<Image>().color = tempValue < 0 ? Color.white : noColor;

        StartCoroutine(ChangeFloor(tempValue, from, to));
    }
    IEnumerator ChangeFloor(int val, int from, int to)
    {
        yield return new WaitForSeconds(waitTime);
        currentFloor += val;
        floorCounter.text = floorNames[^(currentFloor + 1)];
        from += val;
        if (from == to)
        {
            //load or unload passangers
        }
        else
        {
            MoveElevator(from, to);
        }
    }
}
