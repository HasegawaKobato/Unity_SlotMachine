using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    GameController gameController;
    Row row;
    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        row = gameObject.GetComponentInParent<Row>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (row.getScrolling())
        {
            gameObject.transform.Translate(Vector2.down * Time.fixedDeltaTime * row.GetScrollSpeed());
            if (gameObject.transform.position.y < gameController.minY)
            {
                Vector3 newPos = gameObject.transform.position;
                newPos.y = newPos.y + (gameController.maxY - gameController.minY);
                gameObject.transform.position = newPos;
            }
        }

    }

}
