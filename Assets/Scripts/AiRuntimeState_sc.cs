using UnityEngine;
public class AiRuntimeState_sc : MonoBehaviour
{
    public static AiRuntimeState_sc Instance { get; private set; }

    [TextArea(3, 10)]
    public string loadedAiJson; //AI Yok

    public bool HasAi => !string.IsNullOrEmpty(loadedAiJson);

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ClearAi() => loadedAiJson = null;

    public void SetAiJson(string json) => loadedAiJson = json;
}
