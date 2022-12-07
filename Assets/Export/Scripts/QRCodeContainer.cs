using Microsoft.MixedReality.OpenXR;
using UnityEngine;

public class QRCodeContainer : MonoBehaviour
{
    public SpatialGraphNode node;

    private void Update()
    {
        if (node == null)
        {
            return;
        }

        if (node.TryLocate(FrameTime.OnUpdate, out Pose pose))
        {
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
    }
}
