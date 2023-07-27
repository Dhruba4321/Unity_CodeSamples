/*
-------------------------------------------------------------------------------------
File:         FollowHead.cs
Project:      HistoryVR
Programmer:   Dhruba Karmakar <dhrubakarmakar4321@gmail.com>
First Version:2023-07-27
-------------------------------------------------------------------------------------

Description:
This script is to be attached to the UI object. You need to assign 
the VR player (or camera object) to the VRPlayer public field through the Unity editor. 
The smoothTime variable controls how smoothly the UI follows the VR player.

-------------------------------------------------------------------------------------

Copyright (C) 2023 DigiDrub India Pvt Ltd. All rights reserved.

Unauthorized copying of this file, via any medium is strictly prohibited.
Proprietary and confidential.
*/

using UnityEngine;

namespace HistoryVR
{
    public class FollowHead : MonoBehaviour
    {
        [ColoredHeader(0.6f, 0.0f, 0.0f, "VR Player")]
        [SerializeField] GameObject VRPlayer; // The VR Player
        [SerializeField] float smoothTime = 1.0f; // Smoothing time

        private Vector3 velocity = Vector3.zero; // Velocity used by SmoothDamp function

        // Update is called once per frame
        void Update()
        {
            // Update position
            Vector3 targetPosition = VRPlayer.transform.TransformPoint(new Vector3(0, 0, 20));
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            // Update rotation
            Quaternion targetRotation = VRPlayer.transform.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * smoothTime * 2.0f);
        }
    }
}