#if UNITY_EDITOR

using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class EditorRunnerChecker
{
    [MenuItem("Tools/UniTaskEditorRunnerChecker")]
    public static void RunUniTaskAsync()
    {
        RunCore().Forget();
    }

    static async UniTaskVoid RunCore()
    {
        Debug.Log("Start"); // 打印 "Start" 到 Unity 控制台

        // 下面的代码被注释掉了，因此不会被执行
        //var r = await UnityWebRequest.Get("https://bing.com/").SendWebRequest().ToUniTask();
        //Debug.Log(r.downloadHandler.text.Substring(0, 100));

        // 在当前帧结束前等待一段时间，然后继续执行
        await UniTask.DelayFrame(30);

        Debug.Log("End"); // 打印 "End" 到 Unity 控制台
    }
}

#endif