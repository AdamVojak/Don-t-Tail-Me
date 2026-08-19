using UnityEngine;

public class FlashlightBeamSync : MonoBehaviour
{
    public Light targetLight;
    public Transform beamMesh;
    public float startOffset = 0.1f;

    // Ova metoda se više ne pokreće sama, nego je zove Giovanni
    public void SyncBeam()
    {
        if (targetLight == null || beamMesh == null) return;

        float range = targetLight.range;
        float angleInRadians = (targetLight.spotAngle / 2) * Mathf.Deg2Rad;
        float radius = Mathf.Tan(angleInRadians) * range;

        beamMesh.localScale = new Vector3(radius * 2, range, radius * 2);
        float zOffset = (range / 2f) + startOffset;
        beamMesh.localPosition = new Vector3(0, 0, zOffset);
        beamMesh.localRotation = Quaternion.Euler(-90, 0, 0);
    }
}