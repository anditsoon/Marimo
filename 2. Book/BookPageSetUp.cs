using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookPageSetUp : MonoBehaviour
{
    #region Server

    [SerializeField] private GameObject bookUI;
    [SerializeField] private GameObject ChooseCharacterUI;

    #endregion

    #region Book

    public string text = "";
    private List<string> texts = new();

    private TMP_Text pageNoTxt;
    private int pageNo;

    private TMP_Text leftTxt;
    private TMP_Text rightTxt;
    private RectTransform TextBox;

    [SerializeField] private GameObject img_loadingbar;

    private GameObject titleText;
    private GameObject authorText;
    private GameObject titleImg;

    #endregion

    void Start()
    {
        pageNo = int.Parse(pageNoTxt.text);

        img_loadingbar = GameObject.Find("Img_LoadingBar");

        StartCoroutine(LoadingCoroutine());
    }

    IEnumerator LoadingCoroutine()
    {
        float time = 0;
        while (time < 5f)
        {
            time += Time.deltaTime;
            img_loadingbar.GetComponent<Image>().fillAmount = Mathf.Lerp(img_loadingbar.GetComponent<Image>().fillAmount, 1, time / 2);
            yield return null;
        }
    }

    #region BookUI

    // 텍스트 쪼개기
    public IEnumerator SplitTextIntoPages()
    {
        yield return new WaitForSeconds(2f);

        GameObject.Find("MapSetUpManager").GetComponent<MapSetUp>().ApplyClassMaterial();

        string[] words = text.Split(' ');
        string currentText = "";

        foreach (string word in words)
        {
            string tempText = currentText + (currentText.Length > 0 ? " " : "") + word;

            // 왼쪽 텍스트 박스에 맞는지 확인
            leftTxt.text = tempText;
            Vector2 leftSize = leftTxt.GetPreferredValues(TextBox.rect.size.x, TextBox.rect.size.y);

            if (leftSize.y > TextBox.rect.size.y)
            {
                texts.Add(currentText);
                currentText = word;
            }
            else
            {
                currentText = tempText;
            }
        }

        if (!string.IsNullOrEmpty(currentText))
        {
            texts.Add(currentText);
        }

        leftTxt.text = ""; // PreferredValues 측정하려고 집어넣었던 텍스트들 초기화
        DisplayPage(0);
    }

    // 페이지에 띄우기
    void DisplayPage(int pageIndex)
    {
        pageNoTxt.text = pageIndex.ToString();

        if (pageIndex > 0)
        {
            titleText.SetActive(false);
            authorText.SetActive(false);
            titleImg.SetActive(true);

            leftTxt.text = texts[pageNo * 2 - 2];
            if (pageNo * 2 - 1 < texts.Count)
            {
                rightTxt.text = texts[pageNo * 2 - 1];
            }
            else if (pageNo * 2 > texts.Count)
            {
                rightTxt.text = "";
            }
        }
    }

    // 페이지 왼쪽 넘김
    public void left()
    {
        if (pageNo == 0) return;
        pageNo = Mathf.Max(1, --pageNo);
        SoundManager.instance.PlayEftSound(SoundManager.ESoundType.EFT_BUTTON);
        DisplayPage(pageNo);
    }

    // 페이지 오른쪽 넘김
    public void right()
    {
        if (pageNo + 1 > (texts.Count + 1) / 2) // 더 이상 넘길 페이지가 없으면
        {
            bookUI.SetActive(false);
            SoundManager.instance.PlayEftSound(SoundManager.ESoundType.EFT_BUTTON);
            ChooseCharacterUI.SetActive(true);
        }
        else
        {
            pageNo = Mathf.Min((texts.Count + 1) / 2, ++pageNo);
            SoundManager.instance.PlayEftSound(SoundManager.ESoundType.EFT_BUTTON);
            DisplayPage(pageNo);
        }
    }

    #endregion
}
