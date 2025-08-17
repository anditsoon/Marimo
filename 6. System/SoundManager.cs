using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public enum ESoundType
    {
        EFT_3D_OBJECT_01,
        EFT_3D_OBJECT_02,
        EFT_3D_OBJECT_03,
        EFT_3D_OBJECT_04,
        EFT_3D_OBJECT_05,
        EFT_BOOKMARK,
        EFT_BUTTON,
        EFT_CAMERA,
        EFT_FENCE_ON,
        EFT_FENCE_ON_02,
        EFT_FENCE_ON_03,
        EFT_INFO,
        EFT_KEY,
        EFT_QUIZ_CAMERA,
        EFT_RIGHTANSWER,
        EFT_TIMEATTACK,
        EFT_WRONGANSWER
    }

    public enum EBgmType
    {
        BGM_LOGIN,
        BGM_MAIN,
        BGM_ALBUM
    }


    public static SoundManager instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // audiosource
    public AudioSource eftAudio;
    public AudioSource bgmAudio;

    public AudioClip[] eftAudios;
    public AudioClip[] bgmAudios;

    public void PlayEftSound(ESoundType idx)
    {
        int audioIdx = (int)idx;
        eftAudio.PlayOneShot(eftAudios[audioIdx]);
    }

    public void PlayBgmSound(EBgmType idx)
    {
        int bgmIdx = (int)idx;
        // 플레이할 AudioClip을 설정
        bgmAudio.clip = bgmAudios[bgmIdx];
        bgmAudio.Play();
    }

    public void StopBgmSound()
    {
        bgmAudio.Stop();
    }
}
