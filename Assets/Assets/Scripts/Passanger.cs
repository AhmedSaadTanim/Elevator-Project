using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Passanger : MonoBehaviour
{

    public int weight;
    public int destination;
    public bool hasReached;

    [SerializeField] float posX = 3.80f, liftPosX = 1.38f, moveSpeed = 2f;
    [SerializeField] float duration = 20f, fadeoutDuration = 5f;

    ElevatorSystem elevatorSystem;
    MainManager mainManager;
    Color fade = new(0, 0, 0, 0);
    bool elevatorReady, shouldWait;
    string passangerInfo;
    float elapsedTime, waitingPosX;
    int type, waitingFloor;
    private void Awake()
    {
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>();
        elevatorSystem = GameObject.Find("elevatorDisplay").GetComponent<ElevatorSystem>();
    }

    private void Start()
    {
        passangerInfo = transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text;
        FormatInfo();
    }
    private void Update()
    {
        if (hasReached)
        {
            Exit();
        }

        if (elevatorReady)
        {
            transform.position = Vector2.Lerp(transform.position, new Vector2(liftPosX, transform.position.y), moveSpeed * Time.deltaTime);

            if (Mathf.Abs(transform.position.x - liftPosX) < 0.05)
            {
                GetComponent<SpriteRenderer>().sortingOrder = 0;
                transform.GetChild(0).gameObject.SetActive(false);

                elevatorReady = false;
                ElevatorSystem.capacityUsed += weight;
                elevatorSystem.LoadPassangers();
            }
        }

        if(shouldWait)
        {
            transform.position = Vector2.Lerp(transform.position, new Vector2(waitingPosX, transform.position.y), moveSpeed * Time.deltaTime);

            if (Mathf.Abs(transform.position.x - waitingPosX) <= 0.05)
            {
                shouldWait = false;
            }
        }
    }

    public void MoveTo()
    {
        elevatorReady = true;
    }

    private void Exit()
    {
        transform.GetChild(0).gameObject.SetActive(true);

        elapsedTime += Time.deltaTime;
        transform.position = Vector2.Lerp(transform.position, new Vector2(posX, transform.position.y), elapsedTime / duration);

        if (Mathf.Abs(transform.position.x - posX) < 0.05)
        {
            GetComponent<SpriteRenderer>().color = Color.Lerp(GetComponent<SpriteRenderer>().color, fade, fadeoutDuration * Time.deltaTime);
            if (GetComponent<SpriteRenderer>().color.a <= 0.05)
            {
                Destroy(gameObject);
            }
        }
    }

    private void FormatInfo()
    {
        string[] temp = passangerInfo.Split('(', ')');
        weight = int.Parse(temp[0]);
        destination = int.Parse(temp[1]);
        type = transform.name.Contains("VIP") ? 1 : 0;
    }

    public void OnDestinationReached()
    {
        //reseting bools
        hasReached = true;

        //setting passanger properties
        GetComponent<SpriteRenderer>().sortingOrder = 5;
        Vector2 temp = new Vector2(transform.position.x, SpawnManager.spawnYStart + (GameManager.elevatorOnFloor * SpawnManager.floorOffset));
        transform.position = temp;

        //updating elevator system
        ElevatorSystem.capacityUsed -= weight;
    }

    public void WaitInLine(int floor)
    {
        waitingFloor = floor;

        transform.GetChild(0).gameObject.SetActive(true);
        GetComponent<SpriteRenderer>().sortingOrder = 5;
        ElevatorSystem.capacityUsed -= weight;
        shouldWait = true;

        waitingPosX = -SpawnManager.spawnXStart;

        if (mainManager.gameManager.DP[waitingFloor].Count != 0)
        {
            waitingPosX = mainManager.gameManager.DP[floor].First.Value.transform.position.x;
        }

        foreach (GameObject p in mainManager.gameManager.DP[floor])
        {
            p.transform.position = new Vector2(p.transform.position.x - 1, p.transform.position.y);
        }

        transform.position = new Vector2(transform.position.x, SpawnManager.spawnYStart + (GameManager.elevatorOnFloor * SpawnManager.floorOffset));
        mainManager.gameManager.DP[waitingFloor].AddFirst(gameObject);
        ElevatorSystem.floorCall.Enqueue(new(floor, weight));
    }
}
