using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotSeatManager : MonoBehaviourPun
{
    [SerializeField] private GameObject hotSeatCanvas;
    // 애니메이션 오브젝트
    [SerializeField] private GameObject ani_Object;
    [SerializeField] private GameObject virtualCamera;
    
    private float triggerNum = 0;
    private float canvasAlphaTime = 0;
    private bool act = false;
    private List<GameObject> players = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !act)
        {
            triggerNum++;

            players.Add(other.gameObject);

            if (triggerNum >= 4 && !act)
            {
                if(photonView.IsMine)
                {
                    act = true;

                    RPC_MoveControl(false);

                    RPC_AniDelayStart();
                }
            }
        }
    }

    void RPC_ActivateHotSeat()
    {
        photonView.RPC(nameof(ActivateHotSeat), RpcTarget.All);
    }

    [PunRPC]
    void ActivateHotSeat()
    {
        CanvasRenderer[] canvasRenderers = hotSeatCanvas.GetComponentsInChildren<CanvasRenderer>();
        hotSeatCanvas.SetActive(true);
        StartCoroutine(IncreaseAlpha(canvasRenderers));
    }

    public IEnumerator IncreaseAlpha(CanvasRenderer[] canvasRenderers)
    {
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
    }

    void RPC_AniDelayStart()
    {
        photonView.RPC(nameof (AniDelayStart), RpcTarget.All);
    }

    [PunRPC]
    public void AniDelayStart()
    {
        StartCoroutine(AniDelay());
    }

    public IEnumerator AniDelay()
    {
        virtualCamera.SetActive(true);

        ani_Object.SetActive(true);
        SoundManager.instance.PlayEftSound(SoundManager.ESoundType.EFT_3D_OBJECT_03);
        yield return new WaitForSeconds(2f);
        if (photonView.IsMine) RPC_ActivateHotSeat();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !act)
        {
            triggerNum--;

            players.Remove(other.gameObject);
        }
    }

    void RPC_MoveControl(bool canmove)
    {
        photonView.RPC(nameof(MoveControl), RpcTarget.All, canmove);
    }

    [PunRPC]
    public void MoveControl(bool canmove)
    {
        foreach (GameObject obj in players)
        {
            obj.GetComponent<PlayerMove>().Movable = canmove;

            obj.transform.position = new Vector3(obj.transform.position.x, 3, obj.transform.position.z);
        }
    }
}
