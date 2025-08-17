using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAvatarSetting : MonoBehaviour
{
    public PhotonView pv;
    private BookController bookController;
    private BringPlayer bringPlayer;

    [SerializeField] private Sprite[] images;

    public int avatarIndex = -2;

    public Vector3 originalScale; // 플레이어의 원래 스케일 저장

    void Start()
    {
        originalScale = gameObject.transform.localScale;
        pv = GetComponent<PhotonView>();
        bookController = GameObject.Find("BookCanvas").GetComponent<BookController>();
        bringPlayer = GameObject.Find("BookCanvas").GetComponent<BringPlayer>();
        bringPlayer.AddAllPlayer(pv);

        name = pv.Owner.NickName;
    }

    public void RPC_SelectChar(int characterIndex)
    {
        pv.RPC(nameof(SelectChar), RpcTarget.All, characterIndex);
    }

    [PunRPC]
    void SelectChar(int characterIndex)
    {
        avatarIndex = characterIndex - 1;
    }

    public void RPC_UpdatePhoto(int index)
    {
        pv.RPC(nameof(UpdatePhoto), RpcTarget.All, index);
    }

    [PunRPC]
    void UpdatePhoto(int index)
    {
        avatarIndex = index - 1;
        bookController.characterButtons[avatarIndex].GetComponent<Image>().sprite = images[avatarIndex];
    }
}
