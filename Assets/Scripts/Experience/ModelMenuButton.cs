using UnityEngine;

/// <summary>
/// 可选：挂在主菜单按钮上，在未使用目录按顺序绑定时显式指定 Addressables 键。
/// </summary>
public class ModelMenuButton : MonoBehaviour
{
    [SerializeField] private string addressableKey;

    public string AddressableKey => addressableKey;
}
