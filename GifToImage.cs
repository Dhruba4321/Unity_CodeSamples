using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Tropyverse
{
    public class GifToImage : MonoBehaviour
    {
        [SerializeField]
        private Renderer m_renderer;
        [SerializeField]
        private FilterMode m_filterMode = FilterMode.Point;
        [SerializeField]
        private TextureWrapMode m_wrapMode = TextureWrapMode.Clamp;
        [SerializeField]
        private bool m_loadOnStart;
        [SerializeField]
        private string m_loadOnStartUrl;
        [SerializeField]
        private bool m_rotateOnLoading;
        [SerializeField]
        private bool m_outputDebugLog;

        private List<UniGif.GifTexture> m_gifTextureList;
        private float m_delayTime;
        private int m_gifTextureIndex;
        private int m_nowLoopCount;
        private RenderTexture renderTexture;

        public enum State
        {
            None,
            Loading,
            Ready,
            Playing,
            Pause,
        }

        public State nowState { get; private set; }
        public int loopCount { get; private set; }
        public int width { get; private set; }
        public int height { get; private set; }

        private void Start()
        {
            if (m_renderer == null)
            {
                m_renderer = GetComponent<Renderer>();
            }
            if (m_loadOnStart)
            {
                SetGifFromUrl(m_loadOnStartUrl);
            }
        }

        private void OnDestroy()
        {
            Clear();
        }

        private void Update()
        {
            switch (nowState)
            {
                case State.None:
                    break;

                case State.Loading:
                    if (m_rotateOnLoading)
                    {
                        transform.Rotate(0f, 0f, 30f * Time.deltaTime, Space.Self);
                    }
                    break;

                case State.Ready:
                    break;

                case State.Playing:
                    if (m_renderer == null || m_gifTextureList == null || m_gifTextureList.Count <= 0)
                    {
                        return;
                    }
                    if (m_delayTime > Time.time)
                    {
                        return;
                    }
                    m_gifTextureIndex++;
                    if (m_gifTextureIndex >= m_gifTextureList.Count)
                    {
                        m_gifTextureIndex = 0;

                        if (loopCount > 0)
                        {
                            m_nowLoopCount++;
                            if (m_nowLoopCount >= loopCount)
                            {
                                Stop();
                                return;
                            }
                        }
                    }
                    Graphics.Blit(m_gifTextureList[m_gifTextureIndex].m_texture2d, renderTexture);
                    m_delayTime = Time.time + m_gifTextureList[m_gifTextureIndex].m_delaySec;
                    break;

                case State.Pause:
                    break;
            }
        }

        public void SetGifFromUrl(string url, bool autoPlay = true)
        {
            StartCoroutine(SetGifFromUrlCoroutine(url, autoPlay));
        }

        public IEnumerator SetGifFromUrlCoroutine(string url, bool autoPlay = true)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("URL is nothing.");
                yield break;
            }

            if (nowState == State.Loading)
            {
                Debug.LogWarning("Already loading.");
                yield break;
            }
            nowState = State.Loading;

            string path;
            if (url.StartsWith("http"))
            {
                path = url;
            }
            else
            {
                path = Path.Combine("file:///" + Application.streamingAssetsPath, url);
            }

            using (WWW www = new WWW(path))
            {
                yield return www;

                if (string.IsNullOrEmpty(www.error) == false)
                {
                    Debug.LogError("File load error.\n" + www.error);
                    nowState = State.None;
                    yield break;
                }

                Clear();
                nowState = State.Loading;

                yield return StartCoroutine(UniGif.GetTextureListCoroutine(www.bytes, (gifTexList, loopCount, width, height) =>
                {
                    if (gifTexList != null)
                    {
                        m_gifTextureList = gifTexList;
                        this.loopCount = loopCount;
                        this.width = width;
                        this.height = height;
                        nowState = State.Ready;

                        if (m_rotateOnLoading)
                        {
                            transform.localEulerAngles = Vector3.zero;
                        }

                        renderTexture = new RenderTexture(1920, 1080, 16, RenderTextureFormat.ARGB32);
                        m_renderer.material.mainTexture = renderTexture;

                        if (autoPlay)
                        {
                            Play();
                        }
                    }
                    else
                    {
                        Debug.LogError("Gif texture get error.");
                        nowState = State.None;
                    }
                },
                m_filterMode, m_wrapMode, m_outputDebugLog));
            }
        }

        public void Clear()
        {
            if (m_renderer != null)
            {
                m_renderer.material.mainTexture = null;
            }

            if (m_gifTextureList != null)
            {
                for (int i = 0; i < m_gifTextureList.Count; i++)
                {
                    if (m_gifTextureList[i] != null)
                    {
                        if (m_gifTextureList[i].m_texture2d != null)
                        {
                            Destroy(m_gifTextureList[i].m_texture2d);
                            m_gifTextureList[i].m_texture2d = null;
                        }
                        m_gifTextureList[i] = null;
                    }
                }
                m_gifTextureList.Clear();
                m_gifTextureList = null;
            }

            nowState = State.None;
        }

        public void Play()
        {
            if (nowState != State.Ready)
            {
                Debug.LogWarning("State is not READY.");
                return;
            }
            if (m_renderer == null || m_gifTextureList == null || m_gifTextureList.Count <= 0)
            {
                Debug.LogError("Renderer or GIF Texture is nothing.");
                return;
            }
            nowState = State.Playing;
            Graphics.Blit(m_gifTextureList[0].m_texture2d, renderTexture);
            m_delayTime = Time.time + m_gifTextureList[0].m_delaySec;
            m_gifTextureIndex = 0;
            m_nowLoopCount = 0;
        }

        public void Stop()
        {
            if (nowState != State.Playing && nowState != State.Pause)
            {
                Debug.LogWarning("State is not Playing and Pause.");
                return;
            }
            nowState = State.Ready;
        }

        public void Pause()
        {
            if (nowState != State.Playing)
            {
                Debug.LogWarning("State is not Playing.");
                return;
            }
            nowState = State.Pause;
        }

        public void Resume()
        {
            if (nowState != State.Pause)
            {
                Debug.LogWarning("State is not Pause.");
                return;
            }
            nowState = State.Playing;
        }
    }
}