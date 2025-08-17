using Photon.Pun;

public class TeacherMove : MonoBehaviourPun
{
    void Update()
    {
        if (photonView.Owner.IsMasterClient)
        {
            gameObject.transform.position = GetComponent<SetCamera>().PlayerAverage.position;
        }
    }
}
