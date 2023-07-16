using TMPro;
using UnityEngine;

public class Passanger : MonoBehaviour
{
    [HideInInspector] public int weight;
    [HideInInspector] public int destination;
    [HideInInspector] public bool hasReachedDestination;

    [SerializeField] float posX = 3.80f, liftPosX = 1.38f, moveSpeed = 2f;
    [SerializeField] float duration = 20f, fadeoutDuration = 5f;

    ElevatorSystem elevatorSystem;
    MainManager mainManager;
    Color fade = new(0, 0, 0, 0);
    Vector2 movePos;

    bool elevatorReady, startMove;
    string passangerInfo;
    float elapsedTime, waitingPosX;
    int type;

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
        if (hasReachedDestination) { Exit(); }

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

        if(startMove) { Move(movePos); }
    }

    public void Move(Vector2 targetPosition)
    {
        startMove = true;
        movePos = targetPosition;
        transform.position = Vector2.Lerp(transform.position, targetPosition, 2 * moveSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - targetPosition.x) <= 0.05) { startMove = false; }
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
        hasReachedDestination = true;

        //setting passanger properties
        GetComponent<SpriteRenderer>().sortingOrder = 5;
        Vector2 temp = new Vector2(transform.position.x, SpawnManager.spawnYStart + (GameManager.elevatorOnFloor * SpawnManager.floorOffset));
        transform.position = temp;

        //updating elevator system
        ElevatorSystem.capacityUsed -= weight;
    }

    public void WaitInLine(int floor)
    {
        //updating passanger and elevator
        transform.GetChild(0).gameObject.SetActive(true);
        GetComponent<SpriteRenderer>().sortingOrder = 5;
        ElevatorSystem.capacityUsed -= weight;

        //fixing order of standing in the waiting line of the floor
        transform.position = new Vector2(transform.position.x, SpawnManager.spawnYStart + (floor * SpawnManager.floorOffset));
        waitingPosX = mainManager.gameManager.DP[floor].Count != 0 ? mainManager.gameManager.DP[floor].First.Value.transform.position.x
                                                                          : - SpawnManager.spawnXStart;
        Move(new Vector2(waitingPosX, transform.position.y));


        foreach (GameObject p in mainManager.gameManager.DP[floor])
        {
            p.transform.GetComponent<Passanger>().Move(new Vector2(p.transform.position.x - 1, p.transform.position.y));
        }
        
        //Re-adding the new passanger
        mainManager.gameManager.DP[floor].AddFirst(gameObject);
        ElevatorSystem.floorCall.Enqueue(new(floor, weight));
    }
}
