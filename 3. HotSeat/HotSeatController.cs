using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HotSeatController : MonoBehaviourPun
{
    private HotSeatVoiceRecord voiceRecords;
    public BookController bookController;
    public BringPlayer bringPlayer;
    public SelectCharacter selectCharacter;
    
    public GameObject selfIntroduce;
    public GameObject VirtualCamera;
    
    public GameObject[] guides;
    public Sprite[] sprites;
    public Sprite[] guideImages;
    public Image initialGuideImg;
    int guideNum = 0;

    float canvasAlphaTime = 0;
    
    void Start()
    {
        SoundManager.instance.StopBgmSound();
        
        voiceRecords = GetComponent<HotSeatVoiceRecord>();

        // 안내 가이드 띄워주기
        initialGuideImg.gameObject.SetActive(true);

        SoundManager.instance.PlayEftSound(SoundManager.ESoundType.EFT_INFO);
    }

    public void NextGuide()
    {
        guideNum++;
        if (guideNum < 5)
        {
            initialGuideImg.sprite = guideImages[guideNum];
        }
        else
        {
            initialGuideImg.gameObject.SetActive(false);
        }
    }

    public void RPC_ActivateStageGuide(int index)
    {
        photonView.RPC(nameof(ActivateStageGuide), RpcTarget.All, index);
    }

    [PunRPC]
    public void ActivateStageGuide(int index)
    {
        guides[index].SetActive(true);
        SoundManager.instance.PlayEftSound(SoundManager.ESoundType.EFT_INFO);
        StartCoroutine(Deactivate(guides[index]));
    }

    // 오브젝트 2초뒤 꺼주기
    public IEnumerator Deactivate(GameObject gm)
    {
        CanvasRenderer[] canvasRenderers = gm.GetComponentsInChildren<CanvasRenderer>();

        while (true)
        {
            canvasAlphaTime += Time.deltaTime;

            foreach (CanvasRenderer canvasRenderer in canvasRenderers)
            {
                Color originalColor = canvasRenderer.GetColor();
                originalColor.a = canvasAlphaTime;
                canvasRenderer.SetColor(originalColor);
            }

            if (canvasAlphaTime > 1)
            {
                canvasAlphaTime = 0;
                break;
            }

            yield return null;
        }

        if (gm == guides[4]) // 마지막 "참 잘했어요!" UI 의 경우
        {
            yield return new WaitForSeconds(3f);

            // 1초 딜레이
            yield return new WaitForSeconds(2f);
            // 핫시팅 완료
            K_KeyManager.instance.isDoneHotSeating = true;
            K_LobbyUiManager.instance.img_KeyEmptyBox.gameObject.SetActive(true);

            GameObject.Find("Object_HotSeat").GetComponent<HotSeatManager>().MoveControl(true);
            voiceRecords.UnMuteAllPlayers();
            SoundManager.instance.PlayBgmSound(SoundManager.EBgmType.BGM_MAIN);
            // 연출 끝남
            VirtualCamera.SetActive(false);
            gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(2);
            gm.SetActive(false);
        }
    }
}
