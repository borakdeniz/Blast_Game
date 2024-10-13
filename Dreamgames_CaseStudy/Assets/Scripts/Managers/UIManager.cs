using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro; // for textMestPro functionality

public class UIManager : MonoBehaviour
{
    public TMP_Text moveText;
    public TMP_Text moveShadow;

    public TMP_Text boxText;
    public TMP_Text stoneText;
    public TMP_Text vaseText;

    public GameObject stoneGoalCheck;
    public GameObject vaseGoalCheck;
    public GameObject boxGoalCheck;

    public GameObject winScreen;
    public GameObject loseScreen;


    // Start is called before the first frame update
    void Start()
    {
    }

    public void UpdateMovesUI(int moves)
    {
        moveText.text = moves.ToString();
        moveShadow.text = moves.ToString();
    }

    public void UpdateObstacleCount(int box, int stone, int vase)
    {
        boxText.text = box.ToString();
        stoneText.text = stone.ToString();
        vaseText.text = vase.ToString();

        if (!textCheckActivity(box))
        {
            boxText.enabled = false;
            boxGoalCheck.SetActive(true);
        }
        if (!textCheckActivity(stone))
        {
            stoneText.enabled = false;
            stoneGoalCheck.SetActive(true);
        }
        if (!textCheckActivity(vase))
        {
            vaseText.enabled = false;
            vaseGoalCheck.SetActive(true);
        }
 
    }

    private bool textCheckActivity(int count)
    {
        if (count <=0)
        {
            return false;
        }
        return true;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
