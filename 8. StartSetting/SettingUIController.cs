using Photon.Pun;
using UnityEngine;

public class SettingUIController : MonoBehaviour
{
    [SerializeField] private GameObject settings;

    [SerializeField] private GameObject panel_login;
    [SerializeField] private GameObject panel_title;

    public void closeSetting()
    {
        settings.SetActive(false);
    }

    public void clickSetting()
    {
        settings.SetActive(true);
    }

    public void SignOut()
    {
        PhotonNetwork.Disconnect();

        panel_title.SetActive(false);
        panel_login.SetActive(true);
    }
}
