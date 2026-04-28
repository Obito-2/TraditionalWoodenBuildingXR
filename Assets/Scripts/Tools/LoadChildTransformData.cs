using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class LoadChildTransformData : MonoBehaviour
{
    private string filePath;

    void Start()
    {
        // 设置文件路径（假设文件存储在 Application.persistentDataPath）
        filePath = Path.Combine(Application.persistentDataPath, "childTransforms.json");

        // 加载并应用子物体的 transform 数据
        LoadChildTransforms();
    }

    // 加载并应用子物体的 transform 数据
    void LoadChildTransforms()
    {
        // 检查文件是否存在
        if (File.Exists(filePath))
        {
            // 读取文件内容
            string json = File.ReadAllText(filePath);

            // 反序列化 JSON 数据为 TransformDataListWrapper 对象
            TransformDataListWrapper wrapper = JsonUtility.FromJson<TransformDataListWrapper>(json);

            // 如果数据有效，开始应用 transform 数据
            if (wrapper != null && wrapper.transforms != null)
            {
                List<TransformData> transformsList = wrapper.transforms;

                // 确保子物体的数量与数据长度一致
                if (transformsList.Count == transform.childCount)
                {
                    // 遍历每个子物体并应用位置和旋转数据
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        Transform child = transform.GetChild(i);
                        TransformData data = transformsList[i];

                        // 更新子物体的位置和旋转
                        child.position = data.position;
                        child.rotation = data.rotation;
                        //child.name = data.name;
                    }
                }
            }
        }
    }
}

