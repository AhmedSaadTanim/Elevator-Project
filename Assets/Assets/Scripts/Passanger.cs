using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Passanger : MonoBehaviour
{
    public static int weight;
    public bool hasReached;

    [SerializeField] float posX = 3.80f;
    [SerializeField] float duration = 20f, fadeoutDuration = 5f;

    string passangerInfo;
    int destination;
    float elapsedTime;
    Color fade = new(0, 0, 0, 0);
    private void Awake()
    {
        passangerInfo = transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text;
    }

    private void Start()
    {
        FormatInfo();
    }
    private void Update()
    {
        if (hasReached)
        {
            Exit();
        }
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
    }
}
