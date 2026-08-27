using UnityEngine;

public class CameraPickup : MonoBehaviour
{
    [Header("Camera UI")]
    public GameObject cameraUI;

    public void OpenCamera()
    {
        if (cameraUI != null)
        {
            cameraUI.SetActive(true);
        }
    }

    public void CloseCamera()
    {
        if (cameraUI != null)
        {
            cameraUI.SetActive(false);
        }
    }
}