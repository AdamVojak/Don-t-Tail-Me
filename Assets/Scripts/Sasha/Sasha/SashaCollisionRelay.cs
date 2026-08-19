using UnityEngine;

public class SashaCollisionRelay : MonoBehaviour
{
    public SashaController controller;

    private void OnTriggerEnter(Collider other) { if (controller != null) controller.OnTriggerEnter(other); }
    private void OnTriggerExit(Collider other) { if (controller != null) controller.OnTriggerExit(other); }

    //private void OnTriggerStay(Collider other) { if (controller != null) controller.OnTriggerStay(other); }
}