using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSelector : MonoBehaviour
{
    private GameManager gameManager;
    public int colorID;

    private void OnMouseUp() //點擊選取顏色
    {
        GameObject You = GameObject.Find("You");
        You.GetComponent<Player>().PlayerSelectColor(this.colorID);
    }
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
