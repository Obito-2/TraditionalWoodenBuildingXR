#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor 菜单快捷方式，用于快速启动调试自动化流程
/// </summary>
public class DebugAutoPlayHelper
{
    [MenuItem("Tools/Debug/启动自动调试流程")]
    public static void StartAutoDebugPlay()
    {
        UnityEngine.Debug.Log("========== 启动调试流程 ==========");
        UnityEngine.Debug.Log("✓ 进入 Play 模式（确保 DebugAutoPlay 脚本已挂载到场景中）");
        EditorApplication.isPlaying = true;
    }

    [MenuItem("Tools/Debug/停止自动调试流程")]
    public static void StopAutoDebugPlay()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            UnityEngine.Debug.Log("已停止 Play 模式");
        }
    }
}
#endif
