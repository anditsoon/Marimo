using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class SelfIntroduce
{
    public int selfIntNum;
    public int lessonId;
    public string selfIntroduce;
}

public class HttpHotSeatServer : MonoBehaviour
{
    static HttpHotSeatServer instance;

    private BringPlayer bringPlayer;
    private SelectCharacter selectCharacter;

    // 핫시팅 자기소개 전송
    private string sendSelfIntroduceUrl = "api/hot-sitting/self-introduce";
    // 핫시팅 음성파일 전송
    private string sendInterviewWAV = "api/hot-sitting/wav-file";

    public static HttpHotSeatServer GetInstance()
    {
        if (instance == null)
        {
            GameObject go = new GameObject();
            go.name = "HttpHotSeat";
            go.AddComponent<HttpHotSeatServer>();
        }

        return instance;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        bringPlayer = GameObject.Find("BookCanvas").GetComponent<BringPlayer>();
        selectCharacter = GameObject.Find("BookCanvas").GetComponent<SelectCharacter>();
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

    public IEnumerator UploadFileByFormDataWav(HttpInfo info, byte[] wavFile, int selfIntNum)
    {
        // data 를 MultipartForm 으로 셋팅
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("lessonId", HttpRoomSetUp.GetInstance().UserlessonId.ToString()));
        formData.Add(new MultipartFormDataSection("userName", GetUserNickName())); 
        formData.Add(new MultipartFormDataSection("character", GetCharacterName())); 
        formData.Add(new MultipartFormDataSection("selfIntNum", selfIntNum.ToString()));

        formData.Add(new MultipartFormFileSection("wavFile", wavFile, "interview.wav", "audio/wav"));

        using (UnityWebRequest webRequest = UnityWebRequest.Post(info.url, formData))
        {
            webRequest.SetRequestHeader("userId", HttpLogIn.GetInstance().UserId.ToString());

            // 서버에 요청 보내기
            yield return webRequest.SendWebRequest();

            // 요청 결과 상태 확인
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("성공적으로 WAV 파일 업로드");
                Debug.Log("서버 응답: " + webRequest.downloadHandler.text);
            }
            else if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("연결 오류 발생: " + webRequest.error);
            }
            else if (webRequest.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError("데이터 처리 오류 발생: " + webRequest.error);
            }
            else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("프로토콜 오류 발생: " + webRequest.error);
            }
            else
            {
                Debug.LogError("알 수 없는 오류 발생: " + webRequest.error);
            }

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

    public IEnumerator SendSelfIntroduce(int i)
    {
        SelfIntroduce selfIntroduce = new SelfIntroduce
        {
            selfIntNum = i,
            lessonId = HttpRoomSetUp.GetInstance().UserlessonId,
            selfIntroduce = GetSelfIntroduce()
        };

        // JSON 형식으로 변환
        string jsonBody = JsonUtility.ToJson(selfIntroduce);

        // HttpInfo 설정
        HttpInfo info = new HttpInfo
        {
            url = HttpLogIn.GetInstance().mainServer + sendSelfIntroduceUrl,
            body = jsonBody,
            contentType = "application/json",
            onComplete = (DownloadHandler downloadHandler) =>
            {
                Debug.Log("자기소개 보내기 성공: " + downloadHandler.text);
            }
        };

        // Put 메서드 호출
        yield return StartCoroutine(Put(info));
    }

    public void StartSendIntCoroutine(int i)
    {
        StartCoroutine(SendSelfIntroduce(i));
    }

    string GetSelfIntroduce()
    {
        string mySelfIntroduce = GameObject.Find("HotSeatCanvas").GetComponent<HotSeatSelfIntroduce>().selfIntroduceInput.text;
        return mySelfIntroduce;
    }

    public IEnumerator SendInterviewFile(byte[] record, int selfIntNum)
    {
        // HttpInfo 설정
        HttpInfo info = new HttpInfo
        {
            url = HttpLogIn.GetInstance().mainServer + sendInterviewWAV,
            body = "wavFile",
            contentType = "multipart/form-data",
            onComplete = (DownloadHandler downloadHandler) =>
            {
                Debug.Log("WAV 파일 보내기 성공: " + downloadHandler.text);
            }
        };

        yield return StartCoroutine(UploadFileByFormDataWav(info, record, selfIntNum));

    }

    string GetUserNickName()
    {
        return bringPlayer.MyAvatar.pv.Owner.NickName;
    }

    string GetCharacterName()
    {
        return GameObject.Find("HotSeatCanvas").GetComponent<HotSeatSelfIntroduce>().CharacterNames[selectCharacter.characterNum - 1].text;
    }
}
