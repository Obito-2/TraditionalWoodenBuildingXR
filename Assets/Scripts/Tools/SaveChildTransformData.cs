using UnityEngine;
using System.Collections.Generic;
using System.IO;

// 用于存储每个物体的位置和旋转信息
[System.Serializable]
public class TransformData
{
    public string name;
    public Vector3 position;
    public Quaternion rotation;
}

// 用于包裹多个 TransformData 的列表
[System.Serializable]
public class TransformDataListWrapper
{
    public List<TransformData> transforms;
}
public class SaveChildTransformData : MonoBehaviour
{
    // 保存路径
    private string filePath;

    void Start()
    {
        // 设置文件保存路径
        filePath = Path.Combine(Application.persistentDataPath, "childTransforms.json");
        // 保存子物体的 transform 信息
        SaveChildTransforms();
    }

    // 保存子物体的位置和旋转信息
    void SaveChildTransforms()
    {
        List<TransformData> transformsList = new List<TransformData>();

        // 遍历所有直接子物体
        foreach (Transform child in this.transform)
        {
            TransformData data = new TransformData
            {
                name = child.name,
                position = child.position,
                rotation = child.rotation
            };
            transformsList.Add(data);
        }

        // 将数据转为 JSON
        string json = JsonUtility.ToJson(new TransformDataListWrapper { transforms = transformsList });

        // 保存到文件
        File.WriteAllText(filePath, json);

        Debug.Log("Child transforms saved to: " + filePath);
    }
}

