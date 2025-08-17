using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class SendLessonId
{
    public int lessonId;
}

public class SendMaterialId
{
    public int lessonMaterialId;
}

[System.Serializable]
public class UserIdList
{
    public List<int> userIds;
}

[Serializable]
public class ClassMaterial
{
    public string bookTitle;
    public string bookContents;
    public string author;
    public string backgroundUrl;
    public List<Quiz> quizzes;
    public List<OpenQuestion> openQuestions;
    public List<string> lessonRoles;
}

public class HttpRoomSetUp : MonoBehaviourPun
{
    static HttpRoomSetUp instance;

    private List<int> userList = new List<int>();

    public ClassMaterial RealClassMaterial;

    public int UserlessonId;
    public int ClassMaterialId;

    public static HttpRoomSetUp GetInstance()
    {
        if (instance == null)
        {
            GameObject go = new GameObject();
            go.name = "Y_HttpRoomSetUp";
            go.AddComponent<HttpRoomSetUp>();
        }

        return instance;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator Put(HttpInfo info)
    {
        using (UnityWebRequest webRequest = new UnityWebRequest(info.url, "PUT"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(info.body);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("userId", HttpLogIn.GetInstance().UserId.ToString());

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

    public string sendLessonIdUrl = "api/lesson/enter/";

    public IEnumerator SendLessonId(int id)
    {
        SendLessonId sendLessonId = new SendLessonId
        {
            lessonId = UserlessonId
        };

        // JSON 형식으로 변환
        string jsonBody = JsonUtility.ToJson(sendLessonId);

        // HttpInfo 설정
        HttpInfo info = new HttpInfo
        {
            url = HttpLogIn.GetInstance().mainServer + sendLessonIdUrl + sendLessonId.lessonId.ToString(),
            body = jsonBody,
            contentType = "application/json",
            onComplete = (DownloadHandler downloadHandler) =>
            {
                // 수업 데이터 받아오기
                StartCoroutine(GetClassMaterial(id));
            }
        };

        // Put 메서드 호출
        yield return StartCoroutine(Put(info));
    }

    public void RPC_GetUserIds()
    {
        photonView.RPC("GetUserIds", RpcTarget.All);
    }

    [PunRPC]
    public void GetUserIds()
    {
        StartCoroutine(GetUserIdList());
    }

    public IEnumerator GetUserIdList()
    {
        SendLessonId sendLessonId = new SendLessonId
        {
            lessonId = UserlessonId
        };

        string jsonBody = JsonUtility.ToJson(sendLessonId);
        string requestUserListUrl = "api/lesson/participant/";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(HttpLogIn.GetInstance().mainServer + requestUserListUrl + sendLessonId.lessonId.ToString()))
        {
            webRequest.SetRequestHeader("userId", HttpLogIn.GetInstance().UserId.ToString());
            webRequest.downloadHandler = new DownloadHandlerBuffer(); // 서버가 다운로드 할 수 있는 공간 만듦

            // 서버에 요청 보내기
            yield return webRequest.SendWebRequest();

            // 서버 응답 처리
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // JSON 데이터 파싱
                UserIdList userIdList = JsonUtility.FromJson<UserIdList>(webRequest.downloadHandler.text);

                userList = userIdList.userIds;
            }
            else
            {
                Debug.LogError("유저 아이디 리스트 받아오기 실패: " + webRequest.error);
            }
        }
    }

    public IEnumerator GetClassMaterial(int Id)
    {
        SendMaterialId sendMaterialId = new SendMaterialId
        {
            lessonMaterialId = Id
        };

        string jsonBody = JsonUtility.ToJson(sendMaterialId);
        string requestUserListUrl = "api/lesson/";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(HttpLogIn.GetInstance().mainServer + requestUserListUrl + sendMaterialId.lessonMaterialId.ToString()))
        {
            webRequest.SetRequestHeader("userId", HttpLogIn.GetInstance().UserId.ToString());
            webRequest.downloadHandler = new DownloadHandlerBuffer(); // 서버가 다운로드 할 수 있는 공간 만듦

            // 서버에 요청 보내기
            yield return webRequest.SendWebRequest();

            // 서버 응답 처리
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // JSON 데이터 파싱
                ClassMaterial classMaterial = JsonUtility.FromJson<ClassMaterial>(webRequest.downloadHandler.text);

                RealClassMaterial = classMaterial;

                StartCoroutine(GameObject.Find("BookCanvas").GetComponent<BookPageSetUp>().SplitTextIntoPages());
            }
            else
            {
                Debug.LogError("수업자료 받아오기 실패: " + webRequest.error);
            }
        }
    }
}
