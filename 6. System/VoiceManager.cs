using UnityEngine;
using Photon.Voice.Unity;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;
using System;

public class VoiceManager : MonoBehaviour
{
    public static VoiceManager Instance { get; private set; }

    public Recorder recorder;
    public Image voiceIcon;
    public Image noVoiceIcon;

    private int recordingFrequency = 44100;
    private AudioClip currentRecording;

    private void Awake()
    {
        // Singleton 인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        recorder = GetComponent<Recorder>();
    }

    void Update()
    {
        // 만일, M 키를 누르면 음소거한다
        if(Input.GetKeyDown(KeyCode.M))
        {
            recorder.TransmitEnabled = !recorder.TransmitEnabled;
            if(recorder.TransmitEnabled)
            {
                noVoiceIcon.gameObject.SetActive(false);
                voiceIcon.gameObject.SetActive(true);
            }
            else
            {
                voiceIcon.gameObject.SetActive(false);
                noVoiceIcon.gameObject.SetActive(true);
            }
        }
    }

    public void StartRecording(int playerId, int recordingLength)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerId)
        {
            currentRecording = Microphone.Start(null, true, recordingLength, recordingFrequency);
        } 
    }

    public void StopRecording(int playerId, int selfIntNum)
    {
        if (Microphone.IsRecording(null) && PhotonNetwork.LocalPlayer.ActorNumber == playerId)
        {
            int recordingPosition = Microphone.GetPosition(null);
            Microphone.End(null);

            if (currentRecording != null && recordingPosition > 0)
            {
                // 녹음 데이터를 실제 녹음 길이만큼 잘라내기
                AudioClip trimmedRecording = TrimAudioClip(currentRecording, recordingPosition);

                SendAsWav(trimmedRecording, selfIntNum);
            }
        }
    }

    private AudioClip TrimAudioClip(AudioClip clip, int lengthSamples)
    {
        float[] samples = new float[lengthSamples];
        clip.GetData(samples, 0);

        AudioClip trimmedClip = AudioClip.Create(clip.name, lengthSamples, clip.channels, clip.frequency, false);
        trimmedClip.SetData(samples, 0);

        return trimmedClip;
    }

    public void SendAsWav(AudioClip clip, int selfIntNum)
    {
        // AudioClip 데이터 추출
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        byte[] wavData = ConvertToWav(samples, clip.channels, clip.frequency);

        // 통신
        StartCoroutine(HttpHotSeatServer.GetInstance().SendInterviewFile(wavData, selfIntNum));
    }

    private byte[] ConvertToWav(float[] samples, int channels, int frequency)
    {
        MemoryStream stream = new MemoryStream();

        int byteRate = frequency * channels * 2;
        int dataSize = samples.Length * 2;

        // WAV 헤더 작성
        stream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(36 + dataSize), 0, 4);
        stream.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, 4);
        stream.Write(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4);
        stream.Write(BitConverter.GetBytes((short)1), 0, 2);
        stream.Write(BitConverter.GetBytes((short)channels), 0, 2);
        stream.Write(BitConverter.GetBytes(frequency), 0, 4);
        stream.Write(BitConverter.GetBytes(byteRate), 0, 4);
        stream.Write(BitConverter.GetBytes((short)(channels * 2)), 0, 2);
        stream.Write(BitConverter.GetBytes((short)16), 0, 2);

        stream.Write(System.Text.Encoding.ASCII.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(dataSize), 0, 4);

        // 오디오 샘플 데이터를 16-bit PCM으로 변환
        foreach (var sample in samples)
        {
            short intSample = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
            stream.Write(BitConverter.GetBytes(intSample), 0, 2);
        }

        return stream.ToArray();
    }
}
