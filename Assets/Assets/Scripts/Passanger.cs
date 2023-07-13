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
    Color fade = new(0, 0, 0, 0);
    bool elevatorReady;
    string passangerInfo;
    float elapsedTime;
    int type;
    private void Awake()
    {
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
    }

    public void MoveTo()
    {
        elevatorReady = true;
    }

    private void Exit()
    {
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
}
