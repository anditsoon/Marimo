using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class SignUpData
{
    public Role role;
    public string school;
    public int grade;
    public int classRoom;
    public int studentNumber;
    public string name;
    public string password;
}

[System.Serializable]
public class ResponseData
{
    public string userId;
    public string role;
}

// 서버 응답 데이터를 위한 클래스 정의
[System.Serializable]
public class ServerErrorResponse
{
    public string message;
    public int status;
    public string error;
}

public enum Role
{
    TEACHER,
    STUDENT
}

public class HttpLogIn : MonoBehaviour
{
    static HttpLogIn instance;
    
    public string mainServer = "http://211.250.74.75:8202/";
    public string RegisterUrl = "api/user/signup";
    public string logInUrl = "api/user/login";
    
    public bool IsLoggedIn = false;
    public bool IsTeacher;

    public string UserNickName;
    public string UserId;

    [Header("UI 관련 변수들")]
    [SerializeField] private GameObject signUpError;
    [SerializeField] private GameObject signUpUI;
    [SerializeField] private GameObject logInUI;
    [SerializeField] private Sprite[] logInErrorsSprite;
    [SerializeField] private GameObject logInError;
    [SerializeField] private GameObject img_background;


    public static HttpLogIn GetInstance()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("HttpLogIn");
            instance = go.AddComponent<HttpLogIn>();
        }

        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 서버에게 내가 보내는 데이터를 생성
    public IEnumerator Post(HttpInfo info)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Post(info.url, info.body, info.contentType))
        {
            // 서버에 요청 보내기
            yield return webRequest.SendWebRequest();

            // 서버에게 응답이 왔다.
            DoneRequest(webRequest, info);
        }
    }

    void DoneRequest(UnityWebRequest webRequest, HttpInfo info)
    {

        // 만약에 결과가 정상이라면
        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            // 응답 온 데이터를 요청한 클래스로 보내자
            if (info.onComplete != null)
            {
                info.onComplete(webRequest.downloadHandler);
            }
        }
        // 그렇지 않다면 (Error 라면)
        else
        {
            // Error 의 이유를 출력
            Debug.LogError("Net Error : " + webRequest.error);
        }
    }

    public IEnumerator SignUpCoroutine(string username, string password, string school, int grade, int className, int studentNumber, bool isTeacher)
    {
        Role role = Role.STUDENT;

        if (isTeacher)
        {
            role = Role.TEACHER;
        }

        SignUpData registerData = new SignUpData
        {
            role = role,
            school = school,
            grade = grade,
            classRoom = className,
            studentNumber = studentNumber,
            name = username,
            password = password
        };

        string jsonBody = JsonUtility.ToJson(registerData);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonBody);

        // 서버 요청 설정
        using (UnityWebRequest webRequest = new UnityWebRequest(mainServer + RegisterUrl, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend); // 로우 데이터 업로드
            webRequest.downloadHandler = new DownloadHandlerBuffer(); // 서버가 다운로드 할 수 있는 공간 만듦
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // 서버에 요청 보내기
            yield return webRequest.SendWebRequest();

            // 서버 응답 처리
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                signUpUI.SetActive(false);
                logInUI.SetActive(true);
            }
            else
            {
                // 서버 응답 메시지 처리
                string responseText = webRequest.downloadHandler.text;
                var responseJson = JsonUtility.FromJson<ServerErrorResponse>(responseText);

                if (webRequest.responseCode == 500 && responseJson.message.Contains("유저가 이미 존재합니다."))
                {
                    signUpError.SetActive(true);
                    yield return new WaitForSeconds(2f);
                    signUpError.SetActive(false);
                }
            }
        }
    }

    public IEnumerator LogInCoroutine(string username, string password)
    {

        SignUpData registerData = new SignUpData
        {
            name = username,
            password = password
        };

        string jsonBody = JsonUtility.ToJson(registerData);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonBody);

        // 서버 요청 설정
        using (UnityWebRequest webRequest = new UnityWebRequest(mainServer + logInUrl, "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend); // 로우 데이터 업로드
            webRequest.downloadHandler = new DownloadHandlerBuffer(); // 서버가 다운로드 할 수 있는 공간 만듦
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // 서버에 요청 보내기
            yield return webRequest.SendWebRequest();

            // 서버 응답 처리
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                IsLoggedIn = true;
                ResponseData responseData = JsonUtility.FromJson<ResponseData>(webRequest.downloadHandler.text);
                UserId = responseData.userId;

                UserNickName = registerData.name;

                Role userRole;
                Enum.TryParse(responseData.role, true, out userRole);
                IsTeacher = userRole == Role.TEACHER;

                SignUpPageUIController.signUp.LogInUI.SetActive(false);

                if (IsTeacher)
                {
                    SignUpPageUIController.signUp.CreatorUI.SetActive(true);
                    GameObject.Find("Canvas_CreatorTool").GetComponent<P_CreatorToolController>().titleNickname.text = UserNickName;
                    img_background.SetActive(false);
                }
                else
                {
                    SignUpPageUIController.signUp.TitleUI.SetActive(true);
                    SignUpPageUIController.signUp.TitleNickname.text = UserNickName;
                }
            }
            else
            {
                if (webRequest.responseCode == 401)
                {
                    logInError.GetComponent<Image>().sprite = logInErrorsSprite[0];
                }
                else if ((webRequest.result == UnityWebRequest.Result.ConnectionError) || (webRequest.result == UnityWebRequest.Result.ProtocolError))
                {
                    logInError.GetComponent<Image>().sprite = logInErrorsSprite[0];
                }
                else
                {
                    logInError.GetComponent<Image>().sprite = logInErrorsSprite[0];
                }

                logInError.SetActive(true);
                yield return new WaitForSeconds(2f);
                logInError.SetActive(false);
            }
        }
    }
}
