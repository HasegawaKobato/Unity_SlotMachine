using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Item;

public class GameController : MonoBehaviour
{
    public float scrollSpeed = 50f;
    public Sprite[] sprites;
    public int minY = -18;
    public int maxY = 18;
    public GameObject link;
    public GameObject rule;

    Row[] row = new Row[3];
    RectTransform moneyMask;
    Transform[] items = new Transform[39];
    bool scrolling = false, lerpMask = false;
    float timer = 0, maskRight = 0;
    int turnRowIndex = -1, money = 0, bet = 20000;
    int[] blockInfo = new int[9];
    int[,] maps = new int[13, 3] {
        { 0, 3, 6 },
        { 1, 4, 7 },
        { 2, 5, 8 },
        { 0, 4, 8 },
        { 2, 4, 6 },
        { 0, 4, 6 },
        { 2, 4, 8 },
        { 1, 3, 7 },
        { 1, 5, 7 },
        { 0, 4, 7 },
        { 2, 4, 7 },
        { 1, 4, 6 },
        { 1, 4, 8 }
        };
    Sprite[,] result = new Sprite[3, 3];
    Text moneyUI, betUI;
    GameObject canvas, linkRulePanel, howToPlayPanel;
    GameObject[,] resultItemsIndex = new GameObject[3, 3];

    // Start is called before the first frame update
    void Start()
    {
        linkRulePanel = GameObject.Find("LinkRulePanel");
        howToPlayPanel = GameObject.Find("HowToPlayPanel");
        howToPlayPanel.SetActive(false);
        moneyMask = GameObject.FindGameObjectWithTag("Mask").GetComponent<RectTransform>();
        maskRight = moneyMask.offsetMax.x;
        moneyUI = GameObject.FindGameObjectWithTag("Money").GetComponent<Text>();
        betUI = GameObject.FindGameObjectWithTag("Bet").GetComponent<Text>();
        betUI.text = bet.ToString();
        canvas = GameObject.FindGameObjectWithTag("Canvas");
        row[0] = GameObject.FindGameObjectWithTag("Row1").GetComponent<Row>();
        row[1] = GameObject.FindGameObjectWithTag("Row2").GetComponent<Row>();
        row[2] = GameObject.FindGameObjectWithTag("Row3").GetComponent<Row>();
        initRules();
        initPlayerPrefs();
    }


    // Update is called once per frame
    void Update()
    {
        if (scrolling && (Time.time - timer >= 1.5f && turnRowIndex == -1))
        {
            turnRowIndex++;
            timer = Time.time;
        }

        if (lerpMask)
        {
            if ((int)Mathf.Round(maskRight - moneyMask.offsetMax.x) == 0)
            {
                lerpMask = false;
                moneyMask.offsetMax = new Vector2(maskRight, 0f);
            }
            else
            {
                float tempRight = Mathf.Lerp(moneyMask.offsetMax.x, maskRight, 0.1f);
                moneyMask.offsetMax = new Vector2(tempRight, 0f);
            }
        }

    }

    void setResult()
    {
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                int random = Random.Range(0, sprites.Length);
                result[r, c] = sprites[random];
                blockInfo[r * 3 + c] = random;
            }
        }
    }

    void initPlayerPrefs()
    {
        // 金錢
        if (PlayerPrefs.HasKey("money"))
        {
            plusMoney(PlayerPrefs.GetInt("money"));
            if (money < bet) {
                changeBetText(-bet);
            }
        }
        else
        {
            plusMoney(10000000);
        }

        // 日期
        string nowDate = System.DateTime.Now.ToString("yyyy/MM/dd");
        if (PlayerPrefs.HasKey("date"))
        {
            if (System.DateTime.Parse(nowDate).Ticks - System.DateTime.Parse(PlayerPrefs.GetString("date")).Ticks > 0)
            {
                plusMoney(100000);
                PlayerPrefs.SetString("date", nowDate);
            }
        }
        else
        {
            PlayerPrefs.SetString("date", nowDate);
        }
    }

    void initRowItems()
    {
        for (int r = 0; r < row.Length; r++)
        {
            SpriteRenderer[] rowSpriteRenderer = row[r].GetComponentsInChildren<SpriteRenderer>();
            if (rowSpriteRenderer.Length != 13)
            {
                for (int c = 0; c < 3; c++)
                {
                    items[c + 5 + r * 13] = row[r].GetComponentsInChildren<Transform>()[c + 1];
                }
                for (int i = 0; i < 13 - rowSpriteRenderer.Length; i++)
                {
                    int random = Random.Range(0, sprites.Length);
                    GameObject sprite = new GameObject();
                    sprite.name = "sprite";
                    sprite.AddComponent<SpriteRenderer>();
                    sprite.AddComponent<Item>();
                    sprite.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                    sprite.GetComponent<SpriteRenderer>().sprite = sprites[random];
                    sprite.GetComponent<SpriteRenderer>().sortingOrder = 5;
                    sprite.GetComponent<Transform>().localScale = new Vector3(3.0f, 3.0f, 1);
                    sprite.transform.parent = row[r].GetComponent<Transform>();
                    sprite.GetComponent<Transform>().localPosition = i < 5 ? new Vector3(0, (6 - i) * 3, 0) : new Vector3(0, (i - 3) * -3, 0);
                    if (i < 5)
                    {
                        items[i + r * 13] = sprite.GetComponent<Transform>();
                    }
                    else
                    {
                        items[i + 3 + r * 13] = sprite.GetComponent<Transform>();
                    }
                }
                row[r].ToGetComponent();
            }
            else
            {
                for (int s = 0; s < rowSpriteRenderer.Length; s++)
                {
                    Color itemColor = rowSpriteRenderer[s].color;
                    itemColor.a = 1f;
                    rowSpriteRenderer[s].color = itemColor;
                }
            }
        }
        for (int a = 0; a < items.Length; a++)
        {
            items[a].name = a.ToString();
        }
    }

    void initRules()
    {
        GameObject rules = GameObject.FindGameObjectWithTag("Rules");
        float maxWidth = GameObject.Find("Scroll View").GetComponent<RectTransform>().rect.width;
        int tempY = 0, tempX = 0;
        for (int r = 0; r < maps.GetLength(0); r++)
        {
            GameObject newRule = Instantiate(rule);
            newRule.GetComponent<RectTransform>().SetParent(rules.GetComponent<RectTransform>());
            for (int c = 0; c < maps.GetLength(1); c++)
            {
                Image[] circles = newRule.GetComponentsInChildren<Image>();
                circles[maps[r, c] + 1].color = new Color(0, 1, 0, 1);
            }
            float x = 0, y = 0;
            if ((tempX + 1) * 190 >= maxWidth)
            {
                tempX = 0;
                tempY++;
            }
            x = tempX * 190;
            tempX++;
            y = tempY * -190;
            Vector3 newPos = new Vector3(x, y, 0);
            newRule.GetComponent<RectTransform>().anchoredPosition = newPos;
        }
        rules.GetComponent<RectTransform>().sizeDelta = new Vector2(maxWidth, (tempY + 1) * 190);
        linkRulePanel.SetActive(false);
    }

    void plusMoney(int plusValue)
    {
        money += plusValue;
        PlayerPrefs.SetInt("money", money);
        moneyUI.text = money.ToString();
    }

    void changeBetText(int value)
    {
        bet += value;
        betUI.text = bet.ToString();
    }

    void displayHint(string msg)
    {
        GameObject tempLink = Instantiate(link);
        tempLink.GetComponentInChildren<Text>().text = msg;
        tempLink.transform.SetParent(canvas.transform);
        tempLink.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        tempLink.GetComponent<Animator>().SetTrigger("Display");
    }

    void resetBet()
    {
        if (money < bet)
        {
            int targetBet = (int)Mathf.Floor(money / 20000) * 20000;
            if (targetBet == 0)
            {
                displayHint("財產餘額不足。");
            }
            changeBetText(targetBet - bet);
        }
    }

    public void TurnOver()
    {
        int linkCount = 0;
        for (int m = 0; m < maps.GetLength(0); m++)
        {
            bool pass = true;
            int spriteIndex = -1;
            int compareItem = blockInfo[maps[m, 0]];
            for (int i = 1; i < maps.GetLength(1); i++)
            {
                spriteIndex = blockInfo[maps[m, i]];
                if (blockInfo[maps[m, i]] != compareItem)
                {
                    pass = false;
                }
            }
            if (pass)
            {
                linkCount++;
                for (int i = 0; i < maps.GetLength(1); i++)
                {
                    SpriteRenderer itemSprite = resultItemsIndex[(int)Mathf.Floor(maps[m, i] / 3), maps[m, i] % 3].GetComponent<SpriteRenderer>();
                    Color itemColor = itemSprite.color;
                    itemColor.a = 0.5f;
                    itemSprite.color = itemColor;
                }
            }
        }
        if (linkCount > 0)
        {
            displayHint(linkCount + "連線!");
            int gainMoney = (int)Mathf.Round((Mathf.Pow((float)linkCount, 2f) / 2 + linkCount) * bet / 5) + bet;
            plusMoney(gainMoney);
        }
        resetBet();
    }

    public void PlusBet()
    {
        if (bet + 20000 <= money && !scrolling) changeBetText(20000);
    }

    public void MinusBet()
    {
        if (bet - 20000 > 0 && !scrolling) changeBetText(-20000);
    }

    public void LinkRule()
    {
        bool ruleIsActive = linkRulePanel.activeSelf;
        linkRulePanel.SetActive(ruleIsActive ? false : true);
    }

    public void HowToPlay()
    {
        bool howToPlayActived = howToPlayPanel.activeSelf;
        howToPlayPanel.SetActive(howToPlayActived ? false : true);
    }

    public void Scroll()
    {
        if (!scrolling)
        {
            if (bet > 0)
            {
                initRowItems();
                setResult();
                scrolling = true;
                timer = Time.time;
                turnRowIndex = -1;
                for (int r = 0; r < row.Length; r++)
                {
                    row[r].init();
                    row[r].SetRowIndex(r);
                    row[r].SetScrolling(true);
                }
                plusMoney(-bet);
            } else {
                displayHint("財產餘額不足。");
            }
        }
    }

    public void MoneyMask()
    {
        maskRight = (int)Mathf.Round(moneyMask.offsetMax.x) != 0 ? 0 : -230;
        lerpMask = true;
    }

    public int GetTurnRowIndex()
    {
        return turnRowIndex;
    }

    public float GetScrollSpeed()
    {
        return scrollSpeed;
    }

    public bool GetScrolling()
    {
        return scrolling;
    }

    public void SetScrolling(bool value)
    {
        scrolling = value;
    }

    public void replaceResult(int targetIndex, int rowIndex)
    {
        for (int r = 0; r < 3; r++)
        {
            int replaceIndex = targetIndex - (3 - r) >= 0 ? targetIndex - (3 - r) : targetIndex + r + 10;
            replaceIndex = replaceIndex < rowIndex * 13 ? replaceIndex + 13 : replaceIndex;
            items[replaceIndex].GetComponent<SpriteRenderer>().sprite = result[rowIndex, r];
            resultItemsIndex[rowIndex, r] = items[replaceIndex].gameObject;
        }
    }

    public bool isStopped()
    {
        return (!row[0].getScrolling() && !row[1].getScrolling() && !row[2].getScrolling()) ? true : false;
    }

    public void PlusTurnRowIndex()
    {
        turnRowIndex++;
    }

}
