using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Row : MonoBehaviour
{
    GameController gameController;
    Transform[] children;
    float scrollSpeed = 50f, targetScrollSpeed = 0;
    bool scrolling = false, alignItem = false;
    int targetIndex = -1, rowIndex = -1;
    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {

        if (targetIndex != -1)
        {
            if (rowIndex == 0) print (targetIndex + "," + scrollSpeed);
            GameObject targetObject = GameObject.Find(targetIndex.ToString());
            float targetY = targetObject.transform.position.y + 6;
            targetScrollSpeed = targetY < 0 ? -1 : targetY < 6 ? 5 : targetY < 10 ? 10 : targetScrollSpeed;
            if (targetY <= 0 && targetY >= -1f)
            {
                scrollSpeed = 0;
                scrolling = false;
            }
        }

        if (gameController.GetTurnRowIndex() >= rowIndex)
        {
            if (scrolling)
            {
                scrollSpeed = Mathf.Lerp(scrollSpeed, targetScrollSpeed, Time.fixedDeltaTime * 0.5f);
                targetIndex = calculateTargetIndex() != -1 ? calculateTargetIndex() : throw new UnityException("ERROR");
                gameController.replaceResult(targetIndex, rowIndex);
            }
            else if (!alignItem)
            {
                gameController.PlusTurnRowIndex();
                standardPosition();
                if (gameController.isStopped() && gameController.GetScrolling())
                {
                    gameController.TurnOver();
                    gameController.SetScrolling(false);
                }
            }
        }
    }

    int calculateTargetIndex()
    {
        if (targetIndex != -1) return targetIndex;
        for (int t = 1; t < children.Length; t++)
        {
            if (children[t].position.y <= 13 && children[t].position.y > 10)
            {
                return System.Convert.ToInt32(children[t].name);
            }
        }
        return -1;
    }

    void standardPosition()
    {
        Transform[] children = gameObject.GetComponentsInChildren<Transform>();
        for (int c = 1; c < children.Length; c++)
        {
            float newX = children[c].position.x;
            float newY = 0;
            float deltaY = Mathf.Round(3 - (Mathf.Abs(Mathf.Round(children[c].position.y)) % 3) * 2) % 3;
            deltaY = children[c].position.y < 0 ? -deltaY : deltaY;
            newY = Mathf.Round(children[c].position.y - deltaY);
            Vector3 alignPos = new Vector3(newX, newY, 0);
            children[c].position = alignPos;
        }
        alignItem = true;
    }
    public void init()
    {
        scrollSpeed = gameController.GetScrollSpeed();
        targetIndex = -1;
        rowIndex = -1;
        alignItem = false;
    }

    public void ToGetComponent()
    {
        children = gameObject.GetComponentsInChildren<Transform>();
    }

    public float GetScrollSpeed()
    {
        return scrollSpeed;
    }

    public bool getScrolling()
    {
        return scrolling;
    }

    public void SetScrolling(bool value)
    {
        scrolling = value;
    }

    public void SetRowIndex(int index)
    {
        rowIndex = index;
    }

}
