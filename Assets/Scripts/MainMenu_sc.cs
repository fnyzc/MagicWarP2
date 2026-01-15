using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu_sc : MonoBehaviour
{
    public Slider musicSlider;
    public Slider soundEffectSlider;

    public AudioClip menuMusic;

    [Header("AI Load (WebGL)")]
    [Tooltip("Canvas objenizin adı. Eğer Canvas adınız farklıysa burayı değiştirin.")]
    public string webglReceiverObjectName = "Canvas";

    [Tooltip("Opsiyonel: AI durumunu gösterecek bir UI Text")]
    public Text aiStatusText;

    private void Start()
    {
        if (AudioController_sc.Instance != null)
        {
            musicSlider.SetValueWithoutNotify(AudioController_sc.Instance.musicVolume);
            soundEffectSlider.SetValueWithoutNotify(AudioController_sc.Instance.soundEffectVolume);
            AudioController_sc.Instance.PlayMusic(menuMusic);
        }

        EnsureAiRuntimeState();
        UpdateAiStatusUi();
    }

    private void EnsureAiRuntimeState()
    {
        if (AiRuntimeState_sc.Instance != null) return;

        gameObject.AddComponent<AiRuntimeState_sc>();
    }

    private void UpdateAiStatusUi()
    {
        if (aiStatusText == null) return;

        if (AiRuntimeState_sc.Instance != null && AiRuntimeState_sc.Instance.HasAi)
            aiStatusText.text = "AI: Yüklendi";
        else
            aiStatusText.text = "AI: Yüklenmedi (Random)";
    }

    public void OnNewGameButton()
    {
        SceneManager.LoadScene("Game");
    }
    public void OnLoadAiButton()
    {
    #if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("AI yükleme için dosya seçici açılıyor..."); 
        WebGLFilePicker_sc.Open(webglReceiverObjectName, nameof(OnAiJsonLoaded));
    #else
        Debug.LogWarning("AI yükleme sadece WebGL build'de çalışır.");
    #endif
    }

    public void OnAiJsonLoaded(string json)
    {
        EnsureAiRuntimeState();

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("AI JSON boş veya okunamadı.");
            AiRuntimeState_sc.Instance.ClearAi();
            UpdateAiStatusUi();
            return;
        }

        AiRuntimeState_sc.Instance.SetAiJson(json);
        Debug.Log($"AI JSON loaded. Length={json.Length}");

        UpdateAiStatusUi();
    } 

    public void OnMusicSliderChanged(float value)
    {
        if (AudioController_sc.Instance != null)
            AudioController_sc.Instance.musicVolume = value;
    }

    public void OnSoundEffectSliderChanged(float value)
    {
        if (AudioController_sc.Instance != null)
            AudioController_sc.Instance.soundEffectVolume = value;
    }
}
