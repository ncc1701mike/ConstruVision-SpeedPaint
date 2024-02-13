using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Reflection;

public class NumberScrollManager : MonoBehaviour
{
    public ScrollRect[] scrollRects; 
    public TMP_Text numberDisplay;
    public float snapSpeed = 10f;
    private List<int> currentNumbers = new List<int> {0, 0, 0};
    private List <bool> isScrolling = new List<bool> {false, false, false};
    private List<List<TMP_Text>> numberTexts = new List<List<TMP_Text>>();
    
    // Start is called before the first frame update
    void Start()
    {
       InitializeNumberTexts();
       UpdateNumberDisplay(); 
    }

    private void InitializeNumberTexts()
    {
       foreach (var scrollRect in scrollRects)
       {
           var textsInScrollRect = new List<TMP_Text>();
           foreach (Transform child in scrollRect.content)
           {
                var text = child.GetComponent<TMP_Text>();
                if (text != null)
                {
                     textsInScrollRect.Add(text);
                }
           }
           numberTexts.Add(textsInScrollRect);
       }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        for (int i = 0; i < scrollRects.Length; i++)
        {
            if (!isScrolling[i])
            {
                StartCoroutine(SnapToNumber(scrollRects[i], i));
            }
        }
    }

    private IEnumerator SnapToNumber(ScrollRect rect, int index)
    {
        isScrolling[index]= true;

        float normalizedPosition = rect.verticalNormalizedPosition;
        float segmentSize = 1f / 9f;
        int numberIndex = Mathf.RoundToInt(normalizedPosition / segmentSize);
        float targetPosition = numberIndex * segmentSize;

        while (Mathf.Abs(normalizedPosition - targetPosition) > 0.001f)
        {
            normalizedPosition = Mathf.MoveTowards(normalizedPosition, targetPosition, snapSpeed * Time.deltaTime);
            rect.verticalNormalizedPosition = normalizedPosition;
            yield return new WaitForEndOfFrame();
        }

        currentNumbers[index] = numberIndex;
        UpdateNumberDisplay();
        HighlightCurrentNumber(index, numberIndex);

        isScrolling[index] = false; 
    }

    private void UpdateNumberDisplay()
    {
        numberDisplay.text = $"{currentNumbers[0]}{currentNumbers[1]}{currentNumbers[2]}";
    }

    private void HighlightCurrentNumber(int scrollIndex, int numberIndex)
    {
        foreach (var text in numberTexts[scrollIndex])
        {
            text.color = Color.white;
            text.fontSize = 14;
        }

        if (numberIndex < numberTexts[scrollIndex].Count)
        {
            numberTexts[scrollIndex][numberIndex].color = Color.yellow;
            numberTexts[scrollIndex][numberIndex].fontSize = 18;
        }
    }
    
}
