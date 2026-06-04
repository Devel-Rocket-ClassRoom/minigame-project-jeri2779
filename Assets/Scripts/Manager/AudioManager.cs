using UnityEngine;
using UnityEngine.Audio;

// 오디오 인프라(씬 넘어 사는 싱글톤). AudioMixer의 BGM/SE 노출 파라미터를 dB로 제어.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    private const string BGM_KEY = "BGMVolume";
    private const string SE_KEY = "SEVolume";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (SaveManager.Instance == null)
            return;
        SaveData data = SaveManager.Instance.CurrentData;
        SetBGMVolume(data.bgmVolume);
        SetSEVolume(data.seVolume);
    }

    public void SetBGMVolume(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat(BGM_KEY, Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
        if (SaveManager.Instance != null)
            SaveManager.Instance.CurrentData.bgmVolume = value;
    }

    public void SetSEVolume(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat(SE_KEY, Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
        if (SaveManager.Instance != null)
            SaveManager.Instance.CurrentData.seVolume = value;
    }

    public void PlaySE(AudioClip clip)
    {
        if (clip != null && seSource != null)
            seSource.PlayOneShot(clip);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null)
            return;
        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }
}
