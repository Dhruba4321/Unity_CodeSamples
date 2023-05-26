using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ZenFulcrum.EmbeddedBrowser;
using UnityEngine.Networking;
using System.Security.Cryptography;
using System;
using Newtonsoft.Json;

// The Tropyverse namespace is used to group related classes
namespace Tropyverse
{
    // The Billboard class is used to control the display of videos, pictures, and GIFs on the 3D object
    public class Billboard : MonoBehaviour
    {
        #region Fields and Properties

        // URL of the API
        private const string API_URL = "http://localhost:1337/api/tests";

        // The following variables are set via the custom Inspector created in the BillBoardType.cs Editor script
        // The VideoClip, Texture2D, or Sprite to be displayed on the billboard
        private string videoUrl;         // Strapi video API
        private string imageUrl;        // Strapi image API
        private string gifUrl;          // Strapi gif API 
        private string webUrl;          // Strapi Website url

        private string previousVideoUrl;
        private string previousImageUrl;
        private string previousGifUrl;
        private string previousWebUrl;

        private bool fetchAPI = false; // Make it always initially 'false'


        // Enumerator to select the type of content to display on the billboard
        public enum MyOptions
        {
            Video,
            Image,
            GIF,
            Browser
        }

        // Variable to hold the selected option
        public MyOptions options;

        #endregion

        
        /// <summary>
        /// Unity's method called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            // Perform action based on the selected option
            switch (options)
            {
                // This will create all video components at starting
                case MyOptions.Video:
                    BillboardVideo();
                    // Check the video file from the API every 60 seconds to see if it has been updated
                    // Adjust the frequency of these checks according to your needs to optimize the game
                    InvokeRepeating("CheckAndPlayVideo", 0.0f, 60.0f);
                    break;

                // This will create all Image components at starting
                case MyOptions.Image:
                    BillboardTexture();
                    break;

                // This will create all GIF components at starting
                case MyOptions.GIF:
                    // Ensure the renderer component is available
                    if (m_renderer == null)
                    {
                        m_renderer = GetComponent<Renderer>();
                    }

                    // If loadOnStart is true, load the GIF from the specified URL
                    if (m_loadOnStart)
                    {
                        SetGifFromUrl(gifUrl);
                    }

                    // Enable GIF functionality
                    gifEnable = true;
                    break;
                
                // This will create all Browser components at starting
                case MyOptions.Browser:
                    BillboardBrowser();
                    break;
            }
            // Enable api fetching after first frame
            fetchAPI = true;
        }

        #region  Fetch API data and store the video, image, gif, web URLs
            /// <summary>
            /// Coroutine to fetch the data from the API.
            /// </summary>
            private void Awake()
            {
                // Start the coroutine to make a GET request to the API_URL
                StartCoroutine(CheckForUpdatesEveryMinute());
            }

            

            private IEnumerator CheckForUpdatesEveryMinute()
            {
                while (true)
                {
                    StartCoroutine(GetDataFromAPI());
                    // Call the api in every 30sec delay
                    yield return new WaitForSeconds(30);
                }
            }

            private IEnumerator GetDataFromAPI()
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get(API_URL))
                {
                    // Send the request and wait for a response
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.LogError("Error: " + webRequest.error);
                    }
                    else
                    {
                        // Parse the JSON response
                        var jsonResponse = JsonConvert.DeserializeObject<Root>(webRequest.downloadHandler.text);
                        
                        // Get the URLs
                        var attributes = jsonResponse.data[0].attributes;
                        videoUrl = attributes.videoUrl;
                        imageUrl = attributes.imageUrl;
                        gifUrl = attributes.gifUrl;
                        webUrl = attributes.WebUrl;
                        
                        if(!fetchAPI)
                        {
                            previousVideoUrl = videoUrl;
                            previousImageUrl = imageUrl;
                            previousGifUrl = gifUrl;
                            previousWebUrl = webUrl;
                        }
                        // Check if the URLs have changed
                        if (previousVideoUrl != videoUrl && fetchAPI)
                        {
                            Debug.Log("Video URL has changed to: " + videoUrl);
                            // Check whether the video url is valid or not
                            if(Uri.TryCreate(videoUrl, UriKind.Absolute, out Uri vidUriResult) 
                                && (vidUriResult.Scheme == Uri.UriSchemeHttp || vidUriResult.Scheme == Uri.UriSchemeHttps)
                                && new List<string>{ ".mp4", ".avi", ".mov", ".wmv", ".webm" }.Contains(Path.GetExtension(vidUriResult.AbsolutePath).ToLower()))
                            {
                                previousVideoUrl = videoUrl;
                                // Get and Destroy previous videoPlayer and audioSource component
                                VideoPlayer previousVideo = this.gameObject.GetComponent<VideoPlayer>();
                                AudioSource previousAudio = this.gameObject.GetComponent<AudioSource>();
                                
                                Destroy(previousAudio);
                                Destroy(previousVideo);

                                // Create a new video player and audio source
                                BillboardVideo();
                            }
                            // Condition if the link is not valid
                            else
                            {
                                Debug.Log("Invalid Video URL!!!");
                            }
                        }

                        if (previousImageUrl != imageUrl && fetchAPI)
                        {
                            Debug.Log("Image URL has changed to: " + imageUrl);

                            // Check whether the image url is valid or not
                            if(Uri.TryCreate(imageUrl, UriKind.Absolute, out Uri imgUriResult) 
                                && (imgUriResult.Scheme == Uri.UriSchemeHttp || imgUriResult.Scheme == Uri.UriSchemeHttps)
                                && new List<string>{ ".jpg", ".jpeg", ".png", ".tif", ".bmp", ".wbep" }.Contains(Path.GetExtension(imgUriResult.AbsolutePath).ToLower())
                                )
                            {
                                previousImageUrl = imageUrl;
                                BillboardTexture();
                            }
                            // Condition if the link is not valid
                            else
                            {
                                Debug.Log("Invalid Image URL!!!");
                            }
                            
                        }

                        if (previousGifUrl != gifUrl && fetchAPI)
                        {
                            Debug.Log("GIF URL has changed to: " + gifUrl);

                            // Check whether the gif url is valid or not
                            if(Uri.TryCreate(gifUrl, UriKind.Absolute, out Uri imgUriResult) 
                                && (imgUriResult.Scheme == Uri.UriSchemeHttp || imgUriResult.Scheme == Uri.UriSchemeHttps)
                                && Path.GetExtension(imgUriResult.AbsolutePath).Equals(".gif", StringComparison.OrdinalIgnoreCase))
                            {
                                previousGifUrl = gifUrl;
                                SetGifFromUrl(gifUrl);
                            }
                            // Condition if the link is not valid
                            else
                            {
                                Debug.Log("Invalid GIF URL!!!");
                            }
                        }

                        if (previousWebUrl != webUrl && fetchAPI)
                        {
                            Debug.Log("Web URL has changed to: " + webUrl);

                            // Check whether the url is valid web url or not
                            if (Uri.TryCreate(webUrl, UriKind.Absolute, out Uri webUriResult) 
                                && (webUriResult.Scheme == Uri.UriSchemeHttp || webUriResult.Scheme == Uri.UriSchemeHttps))
                            {
                                // Get and Destroy old 'Browser' component and create a new one with the updated link
                                previousWebUrl = webUrl;
                                Browser previousBrowser = this.gameObject.GetComponent<Browser>();
                                Destroy(previousBrowser);
                                BillboardBrowser();
                            }
                            // Condition if the link is not valid
                            else
                            {
                                Debug.Log("Invalid Web URL!!!");
                            }
                        }
                    }
                }

                
            }

            /// <summary>
            /// Root class to help parse the JSON response.
            /// </summary>
            public class Root
            {
                public Data[] data { get; set; }
            }

            /// <summary>
            /// Root class to help parse the JSON response.
            /// </summary>
            public class Attributes
            {
                public string gifUrl { get; set; }
                public string WebUrl { get; set; }
                public string videoUrl { get; set; }
                public string imageUrl { get; set; }
            }

            /// <summary>
            /// Data class to help parse the JSON response.
            /// </summary>
            public class Data
            {
                public Attributes attributes { get; set; }
            }

        #endregion

        #region Function to display video on the 3D object's material

            // Enable or Disable looping for the video
            [HideInInspector] public bool looping = true;

            public void BillboardVideo()
            {
                if(options == MyOptions.Video)
                {
                    StartCoroutine(PlayVideoFromURL());
                }
            }

            private IEnumerator PlayVideoFromURL()
            {
                var videoPlayer = gameObject.AddComponent<VideoPlayer>();
                var audioSource = gameObject.AddComponent<AudioSource>();

                // Set the AudioSource to 3D
                audioSource.spatialBlend = 1.0f;

                // Set the volume rolloff to linear
                audioSource.rolloffMode = AudioRolloffMode.Linear;

                // Set minimum and maximum distances for 3D audio rolloff
                audioSource.minDistance = 1.0f; // this could be closer depending on your needs
                audioSource.maxDistance = 15.0f; // this could be further depending on your needs

                // Create a new RenderTexture
                RenderTexture renderTexture = new RenderTexture(1920, 1080, 16, RenderTextureFormat.ARGB32);

                // Assign the RenderTexture to the targetTexture property of VideoPlayer
                videoPlayer.targetTexture = renderTexture;

                videoPlayer.playOnAwake = true;
                videoPlayer.source = VideoSource.Url;
                videoPlayer.url = videoUrl;
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                videoPlayer.isLooping = looping;
                videoPlayer.SetTargetAudioSource(0, audioSource);

                // Prepare the Video to prevent buffering
                videoPlayer.Prepare();

                // Wait until video is prepared
                WaitForSeconds waitTime = new WaitForSeconds(1);
                while (!videoPlayer.isPrepared)
                {
                    yield return waitTime;
                }

                // Get the Renderer component and set the main texture to the RenderTexture
                Renderer renderer = GetComponent<Renderer>();
                Material material = renderer.material;
                material.mainTexture = renderTexture;

                // Enable emission
                material.SetTexture("_EmissionMap", renderTexture);
                material.SetFloat("_Smoothness", 0f);
                // Set emission color to white
                renderer.material.SetColor("_EmissionColor", Color.white);

                // Enable emission for the material
                renderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                renderer.material.EnableKeyword("_EMISSION");

                videoPlayer.Play();
            }


        #endregion

        #region  Function to display an image on the 3D object's material
            /// <summary>
            /// This will handles the fetching of an image from a URL, and applying it as a texture to a 3D object.
            /// </summary>    

            public void BillboardTexture()
            {
                if(options == MyOptions.Image)
                {
                    StartCoroutine(FetchTexture());
                }
            }

            /// <summary>
            /// This coroutine fetches a texture from the specified URL, checks the request's success, and then applies
            /// the fetched texture to the object's material. The material is set up for emission with the fetched texture
            /// as the emission map and white as the emission color.
            /// </summary>
            /// <returns>
            /// Yield instruction that pauses the execution of the coroutine until the web request is done.
            /// </returns>
            IEnumerator FetchTexture()
            {
                using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl))
                {
                    yield return www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;

                        // Set main texture
                        Renderer renderer = GetComponent<Renderer>();
                        renderer.material.mainTexture = myTexture;

                        // Enable emission for the material
                        renderer.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                        renderer.material.EnableKeyword("_EMISSION");

                        // Set emission texture
                        renderer.material.SetTexture("_EmissionMap", myTexture);

                        // Set emission color to white
                        renderer.material.SetColor("_EmissionColor", Color.white);
                    }
                }
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
        [HideInInspector] 
        public bool gifEnable;

        

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
            if(options == MyOptions.GIF)
            {
                StartCoroutine(SetGifFromUrlCoroutine(url, autoPlay));
            }
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
            if(options == MyOptions.Browser)
            {
                // Add the Browser component to the current GameObject at runtime
                Browser browser = this.gameObject.AddComponent<Browser>();

                // Set the url property on the newly added component
                browser.Url = webUrl;

                // Get the Renderer component and set the main texture to the RenderTexture
                Renderer renderer = GetComponent<Renderer>();
                Material material = renderer.material;
                material.mainTexture = renderTexture;
                // Enable emission
                material.SetTexture("_EmissionMap", renderTexture);
                material.SetFloat("_Smoothness", 0f);
                // Set emission color to white
                renderer.material.SetColor("_EmissionColor", Color.white);
            }
        }
        #endregion

    }
}
