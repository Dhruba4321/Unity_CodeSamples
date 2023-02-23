//Add this script to a Sptite Image(Map Image)
//Add Scroll Rect & Rect Mask 2D to the Image parent and assign Image to Rect scroll

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Tropyverse
{
    public class MapScrollPan : MonoBehaviour, IScrollHandler
    {
        #region Fields

        [Tooltip("The speed at which the map zooms in and out")]
        [SerializeField] private float zoomSpeed = 0.1f;

        [Tooltip("The maximum allowed zoom level for the map")]
        [SerializeField] private float maxZoom = 10f;

        private Vector3 initialScale;

        #endregion

        #region Methods

        private void Awake()
        {
            initialScale = transform.localScale;
        }

        public void OnScroll(PointerEventData eventData)
        {
            var delta = Vector3.one * (eventData.scrollDelta.y * zoomSpeed);
            var desiredScale = transform.localScale + delta;

            desiredScale = ClampDesiredScale(desiredScale);

            transform.localScale = desiredScale;
        }

        private Vector3 ClampDesiredScale(Vector3 desiredScale)
        {
            desiredScale = Vector3.Max(initialScale, desiredScale);
            desiredScale = Vector3.Min(initialScale * maxZoom, desiredScale);
            return desiredScale;
        }

        #endregion
    }
}
