using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CodeArchitect.Manager.Image
{
    /// <summary>
    /// 网络图片异步加载器，支持缓存
    /// </summary>
    public class ImageLoader : MonoBehaviour
    {
        public static ImageLoader Instance { get; private set; }

        private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 加载网络图片或本地路径的图片
        /// </summary>
        /// <param name="imageUrl">图片 URL（网络或本地路径）</param>
        /// <param name="callback">加载完成回调，返回 Texture2D 或 null</param>
        public void LoadImage(string imageUrl, Action<Texture2D> callback)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                callback?.Invoke(null);
                return;
            }

            // 检查缓存
            if (textureCache.ContainsKey(imageUrl))
            {
                callback?.Invoke(textureCache[imageUrl]);
                return;
            }

            StartCoroutine(LoadImageCoroutine(imageUrl, callback));
        }

        private IEnumerator LoadImageCoroutine(string imageUrl, Action<Texture2D> callback)
        {
            Texture2D texture = null;

            // 判断是否为网络 URL
            if (imageUrl.StartsWith("http://") || imageUrl.StartsWith("https://"))
            {
                yield return StartCoroutine(LoadNetworkImage(imageUrl, (result) =>
                {
                    texture = result;
                }));
            }
            else
            {
                // 尝试加载本地图片
                texture = LoadLocalImage(imageUrl);
            }

            // 缓存结果
            if (texture != null)
            {
                textureCache[imageUrl] = texture;
            }

            callback?.Invoke(texture);
        }

        private IEnumerator LoadNetworkImage(string url, System.Action<Texture2D> callback)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            Texture2D texture = null;
            if (request.result == UnityWebRequest.Result.Success)
            {
                texture = DownloadHandlerTexture.GetContent(request);
                Debug.Log($"[ImageLoader] 网络图片加载成功: {url}");
            }
            else
            {
                Debug.LogError($"[ImageLoader] 网络图片加载失败: {url}\n{request.error}");
            }

            request.Dispose();
            callback?.Invoke(texture);
        }

        private Texture2D LoadLocalImage(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Debug.LogWarning($"[ImageLoader] 本地图片不存在: {path}");
                return null;
            }

            try
            {
                byte[] fileData = System.IO.File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(1, 1);
                if (texture.LoadImage(fileData))
                {
                    Debug.Log($"[ImageLoader] 本地图片加载成功: {path}");
                    return texture;
                }
                else
                {
                    Destroy(texture);
                    Debug.LogWarning($"[ImageLoader] 本地图片加载失败: {path}");
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ImageLoader] 加载本地图片异常: {path}\n{e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void ClearCache()
        {
            foreach (var texture in textureCache.Values)
            {
                if (texture != null)
                    Destroy(texture);
            }
            textureCache.Clear();
        }

        private void OnDestroy()
        {
            ClearCache();
        }
    }
}
