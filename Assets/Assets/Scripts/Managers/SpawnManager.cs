using TMPro;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static float spawnYStart = 1.48f, floorOffset = 4f;

    [SerializeField] float spawnXStart = 0.16f;
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

    private void Generate(int type, int floorNo)
    {
        int destination = inputPanels[floorNo].transform.GetChild(0).GetComponent<DropDownHandler>().destination;
        int weight = int.Parse(inputPanels[floorNo].transform.GetChild(1).GetComponent<TMP_InputField>().text);

        int val = mainManager.gameManager.DP[floorNo].Count + mainManager.gameManager.VIP[floorNo].Count;

        if (val < maxSpawnLimit)
        {
            //Instantiating passangers and updating their informations
            GameObject passanger = Instantiate(passangerPrefabs[type], new Vector2(-(spawnXStart + val), spawnYStart + (floorNo * floorOffset)), Quaternion.identity);
            passanger.transform.SetParent(floors[floorNo].transform);
            passanger.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = weight.ToString() + "(" + destination.ToString() + ")";
            

            if (destination == floorNo)
            {
                passanger.GetComponent<Passanger>().hasReached = true;
            }
            else
            {
                //Calling the elevator
                ElevatorSystem.floorCall.Enqueue(floorNo);

                //adding passanger to floor queue/linkedlist
                if (type == 0)
                {
                    mainManager.gameManager.DP[floorNo].AddLast(passanger);
                }
                else
                {
                    mainManager.gameManager.VIP[floorNo].Enqueue(passanger);
                }
            }
            
        }
    }
}
