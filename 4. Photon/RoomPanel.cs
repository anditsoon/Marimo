using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;

public class RoomPanel : MonoBehaviour
{
    public Button btn_Room;

    public void SetRoomInfo(RoomInfo room)
    {
        btn_Room.GetComponentInChildren<TMP_Text>().text = room.Name;
    }
}