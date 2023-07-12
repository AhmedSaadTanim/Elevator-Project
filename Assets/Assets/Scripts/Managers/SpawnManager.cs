using TMPro;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] float spawnXStart = 0.16f, spawnYStart = 1.48f, floorOffset = 4f;
    [SerializeField] GameObject[] passangerPrefabs;
    [SerializeField] GameObject[] inputPanels;
    [SerializeField] int maxSpawnLimit = 5;

    MainManager mainManager;
    private void Start()
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

            GameObject passanger = Instantiate(passangerPrefabs[type], new Vector2(-(spawnXStart + val), spawnYStart + (floorNo * floorOffset)), Quaternion.identity);
            passanger.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = weight.ToString() + "(" + destination.ToString() + ")";

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
