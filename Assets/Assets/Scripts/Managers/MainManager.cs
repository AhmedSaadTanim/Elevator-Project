using UnityEngine;

public class MainManager : MonoBehaviour
{
    public GameManager gameManager;
    public SpawnManager spawnManager;

    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
    }
}
