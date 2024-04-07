using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycleScript : MonoBehaviour
{
    SpriteRenderer rend;
    [SerializeField] Color[] cycle;
    int index, prevIndex;
    float alpha;
    // Start is called before the first frame update
    void Start()
    {
        prevIndex = 0;
        index = 1;
        rend = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        alpha += Time.deltaTime;
        Color toSet = Color.Lerp(cycle[prevIndex], cycle[index], alpha);
        toSet.a = rend.color.a;
        rend.color = toSet;
        if (alpha >= 1)
        {
            OverAlpha();
        }
    }

    public void OverAlpha()
    {
        index++;
        prevIndex++;
        if (index >= cycle.Length)
        {
            index = 0;
        }

        if (prevIndex >= cycle.Length)
        {
            prevIndex = 0;
        }

        alpha = 0;
    }
}
