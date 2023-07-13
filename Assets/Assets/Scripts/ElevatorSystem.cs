using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ElevatorSystem : MonoBehaviour
{
    [HideInInspector] public static Queue<int> floorCall = new();
    public static int capacityUsed;

    public LinkedList<Passanger> vipQueue = new();
    public LinkedList<Passanger> DPQueue = new();
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
    float elapsedTime;
    bool readyForScan;

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
            if (ScanForPassanger())
            {
                readyForScan = false;
                to = floorCall.Dequeue();
                hasReached = true;
            }
            else if (ScanForDelivery())
            {
                readyForScan = false;

                SetDestinationFloor();
            }
           else if (currentFloor != 0)
           {
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
        ClearFloorCall();
    }


    private void SetDestinationFloor()
    {
        //copying all unique destinations to hashSet
        HashSet<int> tempSet = new();

        Passanger[] temp = vipQueue.ToArray();
        for (int i = 0; i < temp.Length; i++) { tempSet.Add(temp[i].destination); }

        foreach (Passanger p in DPQueue) { tempSet.Add(p.destination); }

        //Debug.Log(vipQueue.Count + " " + DPQueue.Count + " " + tempSet.Count);
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
        if (floorCall.Count > 0)
        {
            //Debug.Log("Entry to clear " + floorCall.Count + " " + to + " " + floorCall.Peek());
            while (floorCall.Count > 0 && to == floorCall.Peek())
            {
                //Debug.Log("Clearing" + floorCall.Peek());
                floorCall.Dequeue();
            }
            //Debug.Log("After clear " + floorCall.Count);
            //if (floorCall.Count > 0)
            // Debug.Log(to + " " + floorCall.Peek());
        }
    }

    private bool ScanForDelivery()
    {
        return vipQueue.Count > 0 || DPQueue.Count > 0;
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

        RegisterInQueue(vipQueue);
        RegisterInQueue(DPQueue);

        if(exitQueue.Count > 0)
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
        if(exitQueue.Count != 0)
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
            Passanger passanger = mainManager.gameManager.VIP[currentFloor].Dequeue().GetComponent<Passanger>();
            passanger.MoveTo();
            vipQueue.AddLast(passanger);
        }
        else if (mainManager.gameManager.DP[currentFloor].Count != 0 && capacityUsed < maxCapacity)
        {
            Passanger passanger = mainManager.gameManager.DP[currentFloor].First.Value.GetComponent<Passanger>();
            mainManager.gameManager.DP[currentFloor].RemoveFirst();
            passanger.MoveTo();
            DPQueue.AddLast(passanger);
        }
        else
        {
            //no passanger on floor, so ready to check for passanger on different floor
            StartCoroutine(DelayOnClose());
        }
    }

    private void UpdateStatusText()
    {
        refVipText.text = vipQueue.Count == 0 ? "" : vipText;
        refWeightText.text = capacityUsed + " KG";
    }

    IEnumerator DelayOnClose()
    {
        yield return new WaitForSeconds(1f);
        elevators[currentFloor].SetTrigger("close");
        readyForScan = true;
    }

    private void RegisterInQueue(LinkedList<Passanger> L)
    {
        if(L.Count > 0)
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
