using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpPageUIController : MonoBehaviour
{
    public static SignUpPageUIController signUp;
    
    [SerializeField] private TMP_InputField[] signUpInputs = new TMP_InputField[6];
    [SerializeField] private TMP_InputField[] logInInputs = new TMP_InputField[2];
    [SerializeField] private bool isTeacher = false;

    [SerializeField] private GameObject signUpUI;
    public GameObject LogInUI;
    public GameObject TitleUI;
    public GameObject CreatorUI;
    public TMP_Text TitleNickname;

    #region Button Sprite

    [SerializeField] private GameObject[] teacherOrStudent;
    [SerializeField] private Button[] buttons;
    [SerializeField] private List<Sprite> sprites = new List<Sprite>();

    #endregion

    private void Awake()
    {
        if (signUp == null)
        {
            signUp = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!isTeacher && (String.IsNullOrEmpty(signUpInputs[0].text) || String.IsNullOrEmpty(signUpInputs[1].text) 
            || String.IsNullOrEmpty(signUpInputs[2].text) || String.IsNullOrEmpty(signUpInputs[3].text) 
            || String.IsNullOrEmpty(signUpInputs[4].text) || String.IsNullOrEmpty(signUpInputs[5].text)))
        {
            buttons[2].GetComponent<Image>().sprite = sprites[4];
            buttons[2].interactable = false;
            return;
        }
        else if (isTeacher && (String.IsNullOrEmpty(signUpInputs[0].text) || String.IsNullOrEmpty(signUpInputs[6].text)
            || String.IsNullOrEmpty(signUpInputs[7].text) || String.IsNullOrEmpty(signUpInputs[4].text)
            || String.IsNullOrEmpty(signUpInputs[5].text)))
        {
            buttons[2].GetComponent<Image>().sprite = sprites[4];
            buttons[2].interactable = false;
            return;
        }
        else
        {
            buttons[2].GetComponent<Image>().sprite = sprites[5];
            buttons[2].interactable = true;
        }
    }

    public void ClickStudent()
    {
        isTeacher = false;
        buttons[0].GetComponent<Image>().sprite = sprites[1];
        buttons[1].GetComponent<Image>().sprite = sprites[2];
        teacherOrStudent[0].SetActive(true);
        teacherOrStudent[1].SetActive(false);
        foreach(TMP_InputField signUpInput in signUpInputs)
        {
            signUpInput.text = "";
        }
    }

    public void ClickTeacher()
    {
        isTeacher = true;
        buttons[0].GetComponent<Image>().sprite = sprites[0];
        buttons[1].GetComponent<Image>().sprite = sprites[3];
        teacherOrStudent[0].SetActive(false);
        teacherOrStudent[1].SetActive(true);
        foreach (TMP_InputField signUpInput in signUpInputs)
        {
            signUpInput.text = "";
        }
    }


    public void ClickConfirm()
    {
        string username = signUpInputs[4].text;
        string password = signUpInputs[5].text;
        string school = signUpInputs[0].text;

        string grade = signUpInputs[1].text;
        string className = signUpInputs[2].text;
        string studentNumber = signUpInputs[3].text;

        if (isTeacher)
        {
            grade = signUpInputs[6].text;
            className = signUpInputs[7].text;
            studentNumber = "1";
        }

        StartCoroutine(HttpLogIn.GetInstance().SignUpCoroutine(username, password, school, Int32.Parse(grade), Int32.Parse(className), Int32.Parse(studentNumber), isTeacher));

        //signUpUI.SetActive(false);
        //logInUI.SetActive(true);
    }

    public void ClickBack()
    {
        signUpUI.SetActive(false);
        LogInUI.SetActive(true);
    }

    public void ClickLogIn()
    {
        string username = logInInputs[0].text;
        string password = logInInputs[1].text;
        StartCoroutine(HttpLogIn.GetInstance().LogInCoroutine(username, password));
    }

    public void ClickJoin()
    {
        LogInUI.SetActive(false);
        signUpUI.SetActive(true);
    }
}
