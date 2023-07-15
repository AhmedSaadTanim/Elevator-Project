using System;
using TMPro;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static float spawnYStart = 1.48f, floorOffset = 4f, spawnXStart = 0.16f;

    [SerializeField] GameObject[] passangerPrefabs;
    [SerializeField] GameObject[] inputPanels, floors;
    [SerializeField] int maxSpawnLimit = 5;

    MainManager mainManager;
    private void Awake()
    {
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>();
    }

    public void OnGenerateDP(int floorNo)
    {
        Generate(0, floorNo);
    }
    public void OnGenerateVIP(int floorNo)
    {
        Generate(1, floorNo);
    }

    private void FixQueue(int type, int floorNo,  Transform passangerTransform)
    {
        if (type == 1 && passangerTransform.position.x != -spawnXStart && mainManager.gameManager.DP[floorNo].Count > 0)
        {
            if(mainManager.gameManager.DP[floorNo].First.Value.transform.position.x > passangerTransform.position.x)
            {
                var lastPassanger = mainManager.gameManager.DP[floorNo].Last;
                while(lastPassanger != null)
                {
                    (passangerTransform.position, lastPassanger.Value.transform.position) = (lastPassanger.Value.transform.position, passangerTransform.position);
                    lastPassanger = lastPassanger.Previous;
                }
            }
        }
    }

    private void Generate(int type, int floorNo)
    {
        int destination = inputPanels[floorNo].transform.GetChild(0).GetComponent<DropDownHandler>().destination;
        int weight = int.Parse(inputPanels[floorNo].transform.GetChild(1).GetComponent<TMP_InputField>().text);

        int val = mainManager.gameManager.DP[floorNo].Count + mainManager.gameManager.VIP[floorNo].Count;

        if (val < maxSpawnLimit)
        {
            //Instantiating passangers and updating their informations
            GameObject passanger = Instantiate(passangerPrefabs[type], new Vector2(-(spawnXStart + val), spawnYStart + (floorNo * floorOffset)), Quaternion.identity);
            FixQueue(type, floorNo, passanger.transform);

            passanger.transform.SetParent(floors[floorNo].transform.Find(FloorQueue.childName + "(Clone)"));
            passanger.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = weight.ToString() + "(" + destination.ToString() + ")";


            if (destination == floorNo)
            {
                passanger.GetComponent<Passanger>().hasReached = true;
            }
            else
            {
                //Calling the elevator
                ElevatorSystem.floorCall.Enqueue(Tuple.Create(floorNo, weight));

                //adding passanger to floor queue/linkedlist
                if (type == 0)
                {
                    mainManager.gameManager.DP[floorNo].AddLast(passanger);
                }
                else
                {
                    mainManager.gameManager.VIP[floorNo].AddLast(passanger);
                }
            }
            
        }
    }
}
