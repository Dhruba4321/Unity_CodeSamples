using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Dhruba
{
    public class WebCameraController : MonoBehaviour
    {
        
        int currentCamIndex = 0;
        WebCamTexture tex;

        [Tooltip("Reference of the Player world space canvas RawImage")]
        public RawImage display;
        

        public void SwapCam_Clicked()
        {
            if(WebCamTexture.devices.Length > 0)
            {
                currentCamIndex += 1;
                currentCamIndex %= WebCamTexture.devices.Length;

                // if tex is not null;
                // stop the webcam
                // start the webcam
                if(tex != null)
                {
                    StopWebCam();
                    StartStopCam_Clicked();
                }
            }
        }

        public void StartStopCam_Clicked()
        {
            if(tex != null) // Stop the Camera
            {
                StopWebCam();
            }
            else // Start the Camera
            {
                WebCamDevice device = WebCamTexture.devices[currentCamIndex];
                tex = new WebCamTexture(device.name);
                display.texture = tex;

                tex.Play();
            }           
        }

        private void StopWebCam()
        {
            display.texture = null;
            tex.Stop();
            tex = null;
        }
    }
}