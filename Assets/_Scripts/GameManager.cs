using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum SwipeType {ShortUp, ShortRight, ShortDown, ShortLeft, LongUp, LongRight, LongDown, LongLeft }
    public float beatTime, distanceMeasure;
    public int beatLength, beatLoops, rounds;
    public SpriteRenderer[] oddDis, evenDis, useBeats;
    public List<SwipeType> order;
    public TextMeshPro scoreDis, displayRead;
    Vector3 startPos;
    [SerializeField] LineRenderer line;
    [SerializeField] Slider willKill;
    [SerializeField] Animator anims;

    AudioSource sounds;
    [SerializeField] AudioClip[] sfx;
    // Start is called before the first frame update
    void Start()
    {
        sounds = GetComponent<AudioSource>();
        SetUpHomeScreen();
    }

    public void SetUpBeats()
    {
        foreach (SpriteRenderer item in oddDis)
        {
            item.gameObject.SetActive(false);
        }

        foreach (SpriteRenderer item in evenDis)
        {
            item.gameObject.SetActive(false);
        }

        useBeats = new SpriteRenderer[beatLength];
        if (beatLength %2 == 0)
        {
            for (int i = 0; i < useBeats.Length; i++)
            {
                useBeats[i] = evenDis[i];
            }
        }
        else
        {
            if (beatLength == 3)
            {
                useBeats[0] = oddDis[1];
                useBeats[1] = oddDis[2];
                useBeats[2] = oddDis[3];
            }
            else if (beatLength == 5)
            {
                for (int i = 0; i < useBeats.Length; i++)
                {
                    useBeats[i] = oddDis[i];
                }
            }
        }

        foreach (SpriteRenderer item in useBeats)
        {
            item.gameObject.SetActive(true);
        }
    }


    int score;
    public void SwipeDone(Vector3 endPos)
    {
        if (!inputOn)
        {
            return;
        }
        if (inputDone)
        {
            score--;
            scoreDis.text = score.ToString();
            return;
        }
        if (PlayerSwipe(endPos) == requestedSwipe)
        {
            score++;
        }
        else
        {
            score--;
        }
        scoreDis.text = score.ToString();
        inputDone = true;
        sounds.PlayOneShot(sfx[(int)PlayerSwipe(endPos)]);
        willKill.value = (float)(score - prevScore) / (float)scoreDiffKill[rounds];
    }


    SwipeType PickSwipe()
    {
        SwipeType hold = (SwipeType)Random.Range(0, 8);
        while (order.Contains(hold))
        {
            hold = (SwipeType)Random.Range(0, 8);
        }
        return hold;
    }


    bool inputOn;
    IEnumerator RoundSetUp()
    {
        SetUpBeats();
        anims.gameObject.GetComponent<SpriteRenderer>().color = Color.white;

        foreach (SpriteRenderer item in useBeats)
        {
            item.color = Color.white;
        }
        order = new List<SwipeType>();
        for (int i = 0; i < beatLength; i++)
        {
            order.Add(PickSwipe());
        }
        loopIndex = 0;

        foreach (SwipeType item in order)
        {
            useBeats[order.IndexOf(item)].color = imCols[loopIndex];
            displayRead.text = "";
            anims.SetTrigger(item.ToString());
            sounds.PlayOneShot(sfx[(int)item]);
            yield return new WaitForSeconds(beatTime);

        }
        loopIndex++;
        inputOn = true;
        inputDone = true;
        for (int i = 0; i < beatLoops; i++)
        {
            StartCoroutine(RequestCycle());
            yield return new WaitForSeconds(beatTime * order.Count + 0.1f);
        }
        anims.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        anims.SetTrigger("Idle");
        inputOn = false;
        rounds++;
        RecalculateValues();
        displayRead.color = Color.white;
        displayRead.text = "Round Over";

        if (CheckGameOver())
        {
            print("Game Over");
            displayRead.text = "Game Over";
            yield return new WaitForSeconds(1f);
            EndGame();
            yield break;
        }

        yield return new WaitForSeconds(1f);
        displayRead.text = "3";
        yield return new WaitForSeconds(0.4f);
        displayRead.text = "2";
        yield return new WaitForSeconds(0.4f);
        displayRead.text = "1";
        yield return new WaitForSeconds(0.4f);


        StartCoroutine(RoundSetUp());
        yield break;
    }

    [SerializeField] Color[] imCols;
    bool inputDone;
    public int cycleIndex, loopIndex;
    float disReadAl = 0.4f;
    SwipeType requestedSwipe;
    public IEnumerator RequestCycle()
    {
        foreach (SwipeType item in order)
        {
            if (!inputDone)
            {
                score--;
                scoreDis.text = score.ToString();
                willKill.value = (float)(score - prevScore) / (float)scoreDiffKill[rounds];
            }

            useBeats[order.IndexOf(item)].color = imCols[loopIndex];
            inputDone = false;
            requestedSwipe = item;
            anims.SetTrigger(item.ToString());
            anims.gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, disReadAl);
            displayRead.text = "";

            yield return new WaitForSeconds(beatTime);
        }
        loopIndex++;
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetMouseButtonDown(0))
            {
                startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 newLine = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                newLine.z = 0;
                line.SetPosition(0, newLine);
            }

            if (Input.GetMouseButtonUp(0))
            {
                SwipeDone(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 newLine = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                newLine.z = 0;
                line.SetPosition(1, newLine);

                Vector3 direction = line.GetPosition(1) - line.GetPosition(0);

                if (direction.magnitude > distanceMeasure)
                {
                    line.endColor = Color.green;
                    line.startColor = Color.yellow;
                }
                else
                {
                    line.endColor = Color.red;
                    line.startColor = Color.blue;
                }
            }
        }

        if (Input.touches.Length > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                startPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                Vector3 newLine = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                newLine.z = 0;
                line.SetPosition(0, newLine);
            }

            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                SwipeDone(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position));
            }

            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Vector3 newLine = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
                newLine.z = 0;
                line.SetPosition(1, newLine);

                Vector3 direction = line.GetPosition(1) - line.GetPosition(0);

                if (direction.magnitude > distanceMeasure)
                {
                    line.endColor = Color.green;
                    line.startColor = Color.yellow;
                }
                else
                {
                    line.endColor = Color.red;
                    line.startColor = Color.blue;
                }
            }
        }
    }

    SwipeType PlayerSwipe(Vector3 endPos)
    {
        Vector3 direction = endPos - startPos;;

        if (direction.magnitude > distanceMeasure)
        {
            if (direction.normalized.x > 0.5f)
            {
                return SwipeType.LongRight;
            }
            else if (direction.normalized.x < -0.5f)
            {
                return SwipeType.LongLeft;
            }
            else if (direction.normalized.y > 0.5f)
            {
                return SwipeType.LongUp;
            }
            else
            {
                return SwipeType.LongDown;
            }
        }
        else
        {
            if (direction.normalized.x > 0.5f)
            {
                return SwipeType.ShortRight;
            }
            else if (direction.normalized.x < -0.5f)
            {
                return SwipeType.ShortLeft;
            }
            else if (direction.normalized.y > 0.5f)
            {
                return SwipeType.ShortUp;
            }
            else
            {
                return SwipeType.ShortDown;
            }
        }
    }

    int prevScore;
    [SerializeField] int[] roundLengths, scoreDiffKill;
    [SerializeField] float[] roundTxtVis, roundBeatTimes;
    void RecalculateValues()
    {
        if (rounds >= roundLengths.Length)
        {
            print("maxRounds");
            return;
        }
        beatLength = roundLengths[rounds];
        disReadAl = roundTxtVis[rounds];
        beatTime = roundBeatTimes[rounds];
    }

    bool CheckGameOver()
    {
        int checkNum = 0;
        if (rounds >= roundLengths.Length)
        {
            checkNum = scoreDiffKill[scoreDiffKill.Length - 1];
        }
        else
        {
            checkNum = scoreDiffKill[rounds];
        }

        if (score - prevScore < checkNum)
        {
            print($"{score - prevScore} + {checkNum}");
            return true;
        }
        else
        {
            prevScore = score;
            return false;
        }
    }

    public bool SaveHighScore()
    {
        if (PlayerPrefs.GetInt("HighScore", 0) < score)
        {
            PlayerPrefs.SetInt("HighScore", score);
            return true;
        }
        else 
        { 
            return false; 
        }
    }

    public GameObject homeScreen;
    public TextMeshProUGUI highScoreMM;
    public void SetUpHomeScreen()
    {
        endScreen.gameObject.SetActive(false);
        homeScreen.gameObject.SetActive(true);
        highScoreMM.text = $"High score:\n{PlayerPrefs.GetInt("HighScore", 0)}";
        line.gameObject.SetActive(false);
    }

    public GameObject endScreen;
    public TextMeshProUGUI highScoredis, endScoreDis, newHS;
    public void EndGame()
    {
        endScreen.gameObject.SetActive(true);
        homeScreen.gameObject.SetActive(false);
        endScoreDis.text = $"You Scored:\n{score.ToString()}";
        if (SaveHighScore())
        {
            newHS.gameObject.SetActive(true);
        }
        else
        {
            newHS.gameObject.SetActive(false);
        }
        highScoredis.text = $"High score:\n{PlayerPrefs.GetInt("HighScore", 0)}";
        line.gameObject.SetActive(false);
    }


    public void PlayGame()
    {
        homeScreen.gameObject.SetActive(false);
        endScreen.SetActive(false);

        score = 0;
        prevScore = 0;
        scoreDis.text = score.ToString();
        rounds = 0;
        highScoreMM.text = $"High score:\n{PlayerPrefs.GetInt("HighScore", 0)}";
        RecalculateValues();
        StartCoroutine(RoundSetUp());
        line.gameObject.SetActive(true);
    }
}
