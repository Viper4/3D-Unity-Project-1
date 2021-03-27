using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class HideOnDistance : MonoBehaviour
{
    public Transform viewer;

    Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    public float visibilityDst = 60;

    // Called before the first frame update
    void Awake()
    {
        if (viewer == null)
        {
            try
            {
                viewer = GameObject.Find("Player").transform;
                Debug.LogWarning("Player Transform used to replace null viewer Transform");
            }
            catch
            {
                Debug.LogError("No Player transform found to replace null viewer Transform");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        if (viewerPosition != viewerPositionOld)
        {
            foreach (Transform child in transform)
            {
                float dstToViewer = Vector3.Distance(viewer.position, child.position);

                if (dstToViewer <= visibilityDst)
                {
                    child.gameObject.SetActive(true);
                }
                else if (child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                }
                if (dstToViewer > visibilityDst * 5f)
                {
                    Destroy(child.gameObject);
                }
                viewerPositionOld = viewerPosition;
            }
        }
    }
}
