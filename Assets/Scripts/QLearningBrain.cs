using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
#endif


public class QLearningBrain : MonoBehaviour
{
    [Header("General Settings")]
    [Range(0f, 1f)] public float learningRate = 0.10f;
    [Range(0f, 1f)] public float discount = 0.95f;

    [Tooltip("Epsilon for exploration when selecting actions (only used when not forcing random).")]
    [Range(0f, 1f)] public float exploration = 0.20f;

    [Header("State Discretization (to avoid infinite states)")]
    [Tooltip("Distance bucket size (input[0])")]
    public float distanceBucket = 1.5f;

    [Tooltip("HP bucket size (input[1] and input[2])")]
    public float hpBucket = 25f;

    [Header("Runtime Mode")]
    [Tooltip("If true, always returns random actions (used when AI wasn't loaded from MainMenu).")]
    public bool forceRandomActions = false;

    [Tooltip("If false, Reward/Punish won't update Q (useful to keep random mode truly random).")]
    public bool learningEnabled = true;

    private List<float> currentInputs = new();
    private readonly List<ActionDefinition> actions = new();

    private readonly Dictionary<string, float[]> Q = new();

    private string lastStateKey = null;
    private int lastActionIndex = -1;
    private bool hasLastTransition = false;

    public bool IsModelLoaded { get; private set; } = false;
    public int ActionCount => actions.Count;

    [Serializable]
    public class SaveModel
    {
        public string model = "QTableV2";
        public int inputCount;
        public int actionCount;
        public List<QEntry> entries = new();
        public string notes;
    }

    [Serializable]
    public class QEntry
    {
        public string state;
        public float[] q; // length = actionCount
    }

    [Serializable]
    public class ActionDefinition
    {
        public string actionName;
        public Action<object[]> method;
        public int parameterCount;
    }

    public void SetInputs(List<float> inputs)
    {
        currentInputs = inputs ?? new List<float>();
    }

    public void RegisterAction(string actionName, Action<object[]> method, int parameterCount)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));
        actions.Add(new ActionDefinition
        {
            actionName = actionName,
            method = method,
            parameterCount = parameterCount
        });
    }

    public int DecideAction()
    {
        if (actions.Count == 0)
        {
            Debug.LogError("QLearningBrain: No actions registered!");
            return 0;
        }

        if (forceRandomActions)
        {
            int rnd = UnityEngine.Random.Range(0, actions.Count);
            RecordLastTransition(rnd);
            return rnd;
        }

        string stateKey = EncodeState(currentInputs);
        EnsureStateExists(stateKey);

        int chosen;
        if (UnityEngine.Random.value < exploration)
            chosen = UnityEngine.Random.Range(0, actions.Count);
        else
            chosen = ArgMax(Q[stateKey]);

        lastStateKey = stateKey;
        lastActionIndex = chosen;
        hasLastTransition = true;

        return chosen;
    }

    public void ExecuteAction(int actionIndex, params object[] parameters)
    {
        if (actionIndex < 0 || actionIndex >= actions.Count) return;
        actions[actionIndex].method?.Invoke(parameters);
    }

    public void Reward(float value) => ApplyRewardInternal(Mathf.Abs(value));
    public void Punish(float value) => ApplyRewardInternal(-Mathf.Abs(value));

    public bool TryLoadFromJson(string json, out string error)
{
    error = null;

    try
    {
        var model = JsonUtility.FromJson<SaveModel>(json);

        if (model.actionCount != actions.Count)
        {
            error = $"Action count mismatch. File:{model.actionCount} Game:{actions.Count}";
            return false;
        }

        Q.Clear();
        foreach (var e in model.entries)
            Q[e.state] = e.q;

        IsModelLoaded = true;

        forceRandomActions = false;
        learningEnabled = false;

        hasLastTransition = false;

        Debug.Log($"[AI] TRAINED MODEL LOADED | States={Q.Count}");
        return true;
    }
    catch (Exception ex)
    {
        error = ex.Message;
        return false;
    }
}


    public string ExportModelJson(string notes = null)
    {
        var model = new SaveModel
        {
            inputCount = currentInputs?.Count ?? 0,
            actionCount = actions.Count,
            notes = notes ?? ""
        };

        foreach (var kv in Q)
            model.entries.Add(new QEntry { state = kv.Key, q = kv.Value });

        return JsonUtility.ToJson(model, true);
    }

    [ContextMenu("DEBUG/Print Model JSON")]
    private void DebugPrintModelJson()
    {
        Debug.Log(ExportModelJson("Exported from Unity"));
    }

    public void ResetToUntrainedRandom()
    {
        Q.Clear();
        IsModelLoaded = false;
        forceRandomActions = true;
        learningEnabled = false;

        hasLastTransition = false;
        lastStateKey = null;
        lastActionIndex = -1;
    }

    private void ApplyRewardInternal(float reward)
    {
        if (!learningEnabled) return;
        if (!hasLastTransition) return;

        string nextStateKey = EncodeState(currentInputs);
        EnsureStateExists(nextStateKey);
        EnsureStateExists(lastStateKey);

        float[] qRow = Q[lastStateKey];
        float maxNext = Max(Q[nextStateKey]);

        qRow[lastActionIndex] =
            qRow[lastActionIndex] + learningRate * (reward + discount * maxNext - qRow[lastActionIndex]);
    }

    private void RecordLastTransition(int actionIndex)
    {
        lastStateKey = EncodeState(currentInputs);
        EnsureStateExists(lastStateKey);
        lastActionIndex = Mathf.Clamp(actionIndex, 0, actions.Count - 1);
        hasLastTransition = true;
    }

    private string EncodeState(List<float> inputs)
    {
        if (inputs == null || inputs.Count == 0) return "S0";

        int d = Quantize(inputs.Count > 0 ? inputs[0] : 0f, distanceBucket);
        int eh = Quantize(inputs.Count > 1 ? inputs[1] : 0f, hpBucket);
        int ph = Quantize(inputs.Count > 2 ? inputs[2] : 0f, hpBucket);

        return $"d{d}_eh{eh}_ph{ph}";
    }

    private int Quantize(float value, float bucket)
    {
        if (bucket <= 0.0001f) return Mathf.RoundToInt(value);
        return Mathf.Clamp(Mathf.FloorToInt(value / bucket), -999, 999);
    }

    private void EnsureStateExists(string stateKey)
    {
        if (string.IsNullOrEmpty(stateKey)) stateKey = "S0";

        if (!Q.ContainsKey(stateKey))
            Q[stateKey] = new float[actions.Count];
    }

    private float Max(float[] arr)
    {
        if (arr == null || arr.Length == 0) return 0f;
        float m = arr[0];
        for (int i = 1; i < arr.Length; i++) if (arr[i] > m) m = arr[i];
        return m;
    }

    private int ArgMax(float[] arr)
    {
        if (arr == null || arr.Length == 0) return 0;
        int best = 0;
        float bestVal = arr[0];
        for (int i = 1; i < arr.Length; i++)
        {
            if (arr[i] > bestVal)
            {
                bestVal = arr[i];
                best = i;
            }
        }
        return best;
    }

    #if UNITY_EDITOR
    [ContextMenu("DEBUG/Export Model JSON To Desktop")]
    private void DebugExportJsonToDesktop()
    {
    var json = ExportModelJson("Exported from Unity");
    var path = System.IO.Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
        "RLBrain.json"
    );
    System.IO.File.WriteAllText(path, json);
    Debug.Log("Saved RLBrain.json to Desktop: " + path);
    }
    #endif

}
