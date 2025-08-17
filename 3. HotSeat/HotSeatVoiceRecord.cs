using Photon.Pun;

public class HotSeatVoiceRecord : MonoBehaviourPun
{
    VoiceManager voiceManager;

    private void Start()
    {
        voiceManager = VoiceManager.Instance;
    }

    [PunRPC]
    public void MuteOtherPlayers(int playerNum)
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient) return;

        int myActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        if (myActorNumber - 1 != playerNum)
        {
            voiceManager.recorder.TransmitEnabled = false;
            voiceManager.noVoiceIcon.gameObject.SetActive(true);
            voiceManager.voiceIcon.gameObject.SetActive(false);
        }
        else
        {
            voiceManager.recorder.TransmitEnabled = true;
            voiceManager.noVoiceIcon.gameObject.SetActive(false);
            voiceManager.voiceIcon.gameObject.SetActive(true);
        }
    }

    [PunRPC]
    public void UnMuteAllPlayers()
    {
        voiceManager.recorder.TransmitEnabled = true;
        voiceManager.noVoiceIcon.gameObject.SetActive(false);
        voiceManager.voiceIcon.gameObject.SetActive(true);
    }

    [PunRPC]
    public void RecordVoice(int i)
    {
        voiceManager.StartRecording(i, 600);
    }

    [PunRPC]
    public void StopRecordVoice(int i, int selfIntNum)
    {
        voiceManager.StopRecording(i, selfIntNum);
    }
}
