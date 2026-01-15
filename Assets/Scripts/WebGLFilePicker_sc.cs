using System.Runtime.InteropServices;
using UnityEngine;
public static class WebGLFilePicker_sc
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void OpenFileDialog(string gameObjectName, string callbackMethod);
#endif

    public static void Open(string receiverGameObjectName, string callbackMethod)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("WebGLFilePicker.Open called.");
        OpenFileDialog(receiverGameObjectName, callbackMethod);
#else
        Debug.LogWarning("WebGLFilePicker.Open called, but not running WebGL build.");
#endif
    }
}
