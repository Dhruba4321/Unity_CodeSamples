using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ZenFulcrum.EmbeddedBrowser;

// The Tropyverse namespace is used to group related classes
namespace Tropyverse
{
    // The Billboard class is used to control the display of videos, pictures, and GIFs on the 3D object
    public class Billboard : MonoBehaviour
    {
        #region Fields and Properties

        // Reference to the main camera in the scene
        private Camera mainCamera;

        // The following variables are set via the custom Inspector created in the BillBoardType.cs Editor script
        // The VideoClip, Texture2D, or Sprite to be displayed on the billboard
        [HideInInspector] public VideoClip videoClip;    // Video file
        [HideInInspector] public Texture2D imageTexture;  // Image file
        [HideInInspector] public bool gifEnable;          // GIF file
        [HideInInspector] public string webUrl;           // Website url


        // Enumerator to select the type of content to display on the billboard
        public enum MyOptions
        {
            Video,
            Picture,
            GIF,
            Browser
        }

        // Variable to hold the selected option
        public MyOptions options;

        #endregion

        private void Start()
        {
            // Get the reference to the main camera
            mainCamera = Camera.main;

            // Perform action based on the selected option
            switch (options)
            {
                case MyOptions.Video:
                    BillboardVideo();
                    break;
                case MyOptions.Picture:
                    BillboardTexture();
                    break;
                case MyOptions.GIF:
                    if (m_renderer == null)
                        {
                            m_renderer = GetComponent<Renderer>();
                        }
                        if (m_loadOnStart)
                        {
                            SetGifFromUrl(m_loadOnStartUrl);
                        }
                    gifEnable = true;
                    break;
                case MyOptions.Browser:
                    BillboardBrowser();
                    break;
            }
        }

        // Orient the gameobject to always face the camera
        private void LateUpdate()
        {
            // Orient the gameobject to face the camera
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);

            // Add 180 degrees rotation to the y-axis
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + 180, transform.rotation.eulerAngles.z);
        }


        #region Methods

        #region Function to display video on the 3D object's material
        public void BillboardVideo()
        {
            var videoPlayer = gameObject.AddComponent<VideoPlayer>();
            var audioSource = gameObject.AddComponent<AudioSource>();

            // Create a new RenderTexture
            RenderTexture renderTexture = new RenderTexture(1920, 1080, 16, RenderTextureFormat.ARGB32);

            // Assign the RenderTexture to the targetTexture property of VideoPlayer
            videoPlayer.targetTexture = renderTexture;

            videoPlayer.playOnAwake = true;
            videoPlayer.clip = videoClip;

            // Set the render mode to VideoRenderMode.RenderTexture
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;

            // Set the AudioSource
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, audioSource);

            // Get the Renderer component and set the main texture to the RenderTexture
            Renderer renderer = GetComponent<Renderer>();
            Material material = renderer.material;
            material.mainTexture = renderTexture;
            // Enable emission
            material.SetTexture("_EmissionMap", renderTexture);
            material.SetFloat("_Smoothness", 0f);
            // Set emission color to white
            renderer.material.SetColor("_EmissionColor", Color.white);

            videoPlayer.Play();
        }
        #endregion

        #region  Function to display an image on the 3D object's material
        public void BillboardTexture()
        {
            Renderer renderer = GetComponent<Renderer>();

            // Set main texture
            renderer.material.mainTexture = imageTexture;

            // Enable emission
            renderer.material.EnableKeyword("_EMISSION");

            // Set emission texture
            renderer.material.SetTexture("_EmissionMap", imageTexture);

            // Set emission color to white
            renderer.material.SetColor("_EmissionColor", Color.white);
        }

        #endregion

        #region Function to display a GIF on the 3D object's material

        /// <summary>
        /// This helps to load GIF from a specified URL and play it on a 3D object's material in Unity3D.
        /// </summary>

        // Public hidden variables to be set in the inspector
        [HideInInspector]
        public Renderer m_renderer;
        [HideInInspector]
        public FilterMode m_filterMode = FilterMode.Point;
        [HideInInspector]
        public TextureWrapMode m_wrapMode = TextureWrapMode.Clamp;
        [HideInInspector]
        public bool m_loadOnStart = true;
        [HideInInspector]
        public bool m_rotateOnLoading;
        [HideInInspector]
        public bool m_outputDebugLog;

        // This URL will be used to fetch the GIF
        [HideInInspector]
        public string m_loadOnStartUrl;

        // List to store textures from each frame of the GIF
        private List<UniGif.GifTexture> m_gifTextureList;
        private float m_delayTime;
        private int m_gifTextureIndex;
        private int m_nowLoopCount;
        private RenderTexture renderTexture;

        // Enum to maintain the current state of the object
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

        // Clear textures on object destruction and set the gifEnable to false
        private void OnDestroy()
        {
            Clear();
            gifEnable = false;
        }

        // Update function to update the state of the object
        private void Update()
        {
            if(gifEnable)
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
        }

        // Function to load GIF from a specified URL
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

                        #region Create a render texture and set it to the BaseMap and Emmission field
                            renderTexture = new RenderTexture(1920, 1080, 16, RenderTextureFormat.ARGB32);
                            m_renderer.material.mainTexture = renderTexture;

                            // Get the Renderer component and set the main texture to the RenderTexture
                            Renderer renderer = GetComponent<Renderer>();
                            Material material = renderer.material;
                            material.mainTexture = renderTexture;
                            // Enable emission
                            material.SetTexture("_EmissionMap", renderTexture);
                            material.SetFloat("_Smoothness", 0f);

                            // Set emission color to white with 50% intensity
                            Color emissionColor = Color.white * 0.5f; 
                            renderer.material.SetColor("_EmissionColor", emissionColor);

                            // Enable emission for the material
                            renderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                            renderer.material.EnableKeyword("_EMISSION");
                        #endregion

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

        // Use this to play gif playback
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

        // Use this to stop gif playback
        public void Stop()
        {
            if (nowState != State.Playing && nowState != State.Pause)
            {
                Debug.LogWarning("State is not Playing and Pause.");
                return;
            }
            nowState = State.Ready;
        }

        // Use this to pause gif playback
        public void Pause()
        {
            if (nowState != State.Playing)
            {
                Debug.LogWarning("State is not Playing.");
                return;
            }
            nowState = State.Pause;
        }

        // Use this to resume gif playback
        public void Resume()
        {
            if (nowState != State.Pause)
            {
                Debug.LogWarning("State is not Pause.");
                return;
            }
            nowState = State.Playing;
        }
        #endregion

        #region Function to display a Browser on the 3D object's material
        public void BillboardBrowser()
        {
            // Add the Browser component to the current GameObject at runtime
            Browser browser = this.gameObject.AddComponent<Browser>();

            // Set the url property on the newly added component
            browser.Url = webUrl;
        }
        #endregion

        #endregion
    }
}
