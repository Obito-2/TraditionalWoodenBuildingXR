using System;

/// <summary>
/// LLM 响应中的图片信息
/// </summary>
[System.Serializable]
public class ImageInfo
{
    public string id;
    public string title;
    public string imageUrl;  // 使用 image_uri
    public string altText;
    public float score;
}
