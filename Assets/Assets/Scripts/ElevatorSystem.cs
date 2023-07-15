using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class ElevatorSystem : MonoBehaviour
{
    [HideInInspector] public static Queue<Tuple<int, int>> floorCall = new();
    public static int capacityUsed;

    public LinkedList<Passanger> vipList = new();
    public LinkedList<Passanger> DpList = new();
    public bool hasReached, noVipOnElevator;
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
    [SerializeField] int maxCapacity = 300;
    [SerializeField] float exitTime = 1f;

    MainManager mainManager;
    Queue<Passanger> exitQueue = new();
    Color noColor = new();
    readonly string vipText = "VIP";
    readonly string[] floorNames = { "2", "1", "G" };
    int currentFloor;
    float elapsedTime, tempWeight;
    bool readyForScan, replace;

    private void Awake()
    {
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>();
    }
    private void Start()
    {
        SwitchToIdle();
        floorCall.Clear();
        floorCounter.text = floorNames[^(currentFloor + 1)];
    }

    private void SwitchToIdle()
    {
        readyForScan = true;
        capacityUsed = 0;
        upArrow.GetComponent<Image>().color = noColor;
        downArrow.GetComponent<Image>().color = noColor;
    }

    private void Update()
    {
        CheckForIdle();
        UpdateStatusText();
        if (readyForScan)
        {
            if (capacityUsed < maxCapacity && ScanForPassanger())
            {
                Debug.Log("Testing");
                readyForScan = false;
                to = floorCall.Dequeue().Item1;
                StartCoroutine(DelayOnStart());
            }
            else if(capacityUsed >= maxCapacity && ScanForPassanger())
            {
                Debug.Log("Entering here");
                readyForScan = false;
                Debug.Log(floorCall.Peek());
                if(mainManager.gameManager.VIP[floorCall.Peek().Item1].Count > 0 && DpList.Count > 0 && calculateReplacableDP(floorCall.Peek().Item1) != 0)
                {
                    Debug.Log("Here too");
                    to = floorCall.Dequeue().Item1;
                    
                    MoveElevator(currentFloor, to, false);
                    //replace = true;
                }
                else if(ScanForDelivery())
                {
                    DoDelivery();
                }
            }
            else if (ScanForDelivery())
            {
                DoDelivery();
            }
            else if (currentFloor != 0)
            {
                Debug.Log("Back");
                readyForScan = false;
                MoveElevator(currentFloor, 0, true);
            }
        }

        if (hasReached)
        {
            hasReached = false;

            if (currentFloor != to)
            {
                MoveElevator(currentFloor, to, false); // to should be decided by checking if passangers are waiting
            }
            else
            {
                elevators[to].SetTrigger("open");
            }
        }

    }


    private void SetDestinationFloor()
    {
        //copying all unique destinations to hashSet
        HashSet<int> tempSet = new();

        foreach (Passanger p in vipList) { tempSet.Add(p.destination); }
        foreach (Passanger p in DpList) { tempSet.Add(p.destination); }

        //Debug.Log(vipList.Count + " " + DpList.Count + " " + tempSet.Count);
        if (tempSet.Count == 2)
        {
            to = currentFloor == 0 ? 1 : currentFloor == 1 ? 2 : 1;
        }
        else
        {
            to = tempSet.Contains(1) ? 1 : tempSet.Contains(2) ? 2 : 0;
        }

        //Debug.Log(to);
        tempSet.Clear();
        MoveElevator(currentFloor, to, false);
    }

    private void ClearFloorCall()
    {
        Debug.Log(floorCall.Count + " " + to);
        if (floorCall.Count > 0)
            Debug.Log(floorCall.Peek());

        tempWeight = 0;
        while (floorCall.Count > 0 && to == floorCall.Peek().Item1)
        {
            tempWeight += floorCall.Peek().Item2;
            //Debug.Log(floorCall.Peek().Item1 + " " + floorCall.Peek().Item2);
            if (tempWeight >= maxCapacity)
            {
                break;
            }
            floorCall.Dequeue();
        }
        
        if (floorCall.Count > 0)
            Debug.Log(floorCall.Peek());
        Debug.Log(floorCall.Count);
    }

    private bool ScanForDelivery()
    {
        return vipList.Count > 0 || DpList.Count > 0;
    }

    private void DoDelivery()
    {
        readyForScan = false;
        SetDestinationFloor();
    }

    private bool ScanForPassanger()
    {
        return floorCall.Count > 0;
    }

    private void CheckForIdle()
    {
        if (currentFloor == 0 && readyForScan)
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

    private void MoveElevator(int from, int to, bool isIdle)
    {
        int tempValue = from < to ? 1 : -1;
        upArrow.GetComponent<Image>().color = tempValue < 0 ? noColor : Color.white;
        downArrow.GetComponent<Image>().color = tempValue < 0 ? Color.white : noColor;

        StartCoroutine(ChangeFloor(tempValue, from, to, isIdle));
    }

    IEnumerator ChangeFloor(int val, int from, int to, bool isIdle)
    {
        yield return new WaitForSeconds(waitTime);
        currentFloor += val;
        floorCounter.text = floorNames[^(currentFloor + 1)];
        from += val;
        if (from == to)
        {
            if (!isIdle)
            {
                elevators[to].SetTrigger("open");
                //animation ending calls UnLoadPassangers()
            }
            else
            {
                readyForScan = true;
            }
        }
        else
        {
            MoveElevator(from, to, isIdle);
        }
    }

    public void UnLoadPassangers()
    {
        //unload passangers if any exists
        GameManager.elevatorOnFloor = currentFloor;

        RegisterInQueue(vipList);
        RegisterInQueue(DpList);

        if (exitQueue.Count > 0)
        {
            StartCoroutine(ExitPassangers());
        }
        else
        {
            LoadPassangers();
        }
    }

    IEnumerator ExitPassangers()
    {
        yield return new WaitForSeconds(exitTime);
        exitQueue.Peek().OnDestinationReached();
        exitQueue.Dequeue();
        if (exitQueue.Count != 0)
        {
            StartCoroutine(ExitPassangers());
        }
        else
        {
            LoadPassangers();
        }
    }

    public void LoadPassangers()
    {
        //load if any passanger is on floor
        if (mainManager.gameManager.VIP[currentFloor].Count != 0 && capacityUsed < maxCapacity)
        {
            Passanger passanger = mainManager.gameManager.VIP[currentFloor].First.Value.GetComponent<Passanger>();
            mainManager.gameManager.VIP[currentFloor].RemoveFirst();
            passanger.MoveTo();
            vipList.AddLast(passanger);
        }
        else if (mainManager.gameManager.DP[currentFloor].Count != 0 && capacityUsed < maxCapacity)
        {
            Passanger passanger = mainManager.gameManager.DP[currentFloor].First.Value.GetComponent<Passanger>();
            mainManager.gameManager.DP[currentFloor].RemoveFirst();
            passanger.MoveTo();
            DpList.AddLast(passanger);
        }
        else if(capacityUsed >= maxCapacity && DpList.Count > 0 && mainManager.gameManager.VIP[currentFloor].Count > 0)
        {
            int counter = calculateReplacableDP(currentFloor);
            if(counter > 0)
            {
                while (counter != 0)
                {
                    DpList.Last.Value.WaitInLine(currentFloor);
                    DpList.RemoveLast();
                    counter--;
                }
                Passanger passanger = mainManager.gameManager.VIP[currentFloor].First.Value.GetComponent<Passanger>();
                mainManager.gameManager.VIP[currentFloor].RemoveFirst();
                passanger.MoveTo();
                vipList.AddLast(passanger);
            }
            else
            {
                Debug.Log("First close");
                StartCoroutine(DelayOnClose());
            }
        }
        else
        {
                Debug.Log("Second close");
            //no passanger on floor, so ready to check for passanger on different floor
            StartCoroutine(DelayOnClose());
        }
    }

    private int calculateReplacableDP(int floorNo)
    {
        int calcWeight = 0;
        int vipWeight = mainManager.gameManager.VIP[floorNo].First.Value.GetComponent<Passanger>().weight;
        int counter = 0;

        var lastNode = DpList.Last;
        while (lastNode != null)
        {
            calcWeight += lastNode.Value.weight;
            counter++;
            if (vipWeight <= calcWeight)
            {
                return counter; 
            }
            lastNode = lastNode.Previous;
        }
        return 0;
    }

    private void UpdateStatusText()
    {
        refWeightText.color = capacityUsed >= maxCapacity ? Color.red : Color.green;
        refVipText.text = vipList.Count == 0 ? "" : vipText;
        refWeightText.text = capacityUsed + " KG";
    }

    IEnumerator DelayOnStart()
    {
        yield return new WaitForSeconds(1f);
        ClearFloorCall();
        hasReached = true;
    }

    IEnumerator DelayOnClose()
    {
        mainManager.gameManager.SortQueue();
        yield return new WaitForSeconds(1f);
        elevators[currentFloor].SetTrigger("close");
        readyForScan = true;
    }

    private void RegisterInQueue(LinkedList<Passanger> L)
    {
        if (L.Count > 0)
        {
            foreach (Passanger p in L.ToList())
            {
                if (p.destination == currentFloor)
                {
                    exitQueue.Enqueue(p);
                    L.Remove(p);
                }
            }
        }
    }
}
