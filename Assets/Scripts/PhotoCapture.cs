using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhotoCapture : MonoBehaviour
{
    [Header("MAIN CAMERA")]
    public Camera mainCamera;

    [Header("CAPTURE")]
    public RenderTexture photoRenderTexture;

    [Header("PHOTO PREVIEW")]
    public GameObject photoPreviewPanel;
    public RawImage photoPreview;
    public float previewTime = 3f;

    [Header("TIMER")]
    public TMP_Text timerText;
    public float roundDuration = 120f;

    [Header("CAMERA CANVAS")]
    public GameObject cameraCanvas;

    [Header("RESULT BOARD")]
    public GameObject resultBoardCanvas;
    public RawImage[] resultPhotoSlots;

    [Header("CAT SPAWNER")]
    public GameObject catSpawner;
    public GameObject timeOverPanel;
    // Stores every photo taken during the round
    private List<Texture2D> capturedPhotos = new List<Texture2D>();

    // Stores whether each photo is valid
    private List<bool> photoValidity = new List<bool>();

    private float remainingTime;
    private bool timerRunning = false;

    private Coroutine previewCoroutine;


    // =========================================================
    // START CAMERA ROUND
    // =========================================================

    public void StartCameraRound()
    {
        Debug.Log("📸 CAMERA ROUND STARTED");

        remainingTime = roundDuration;

        timerRunning = true;

        UpdateTimerDisplay();

        // Start cat spawning
        if (catSpawner != null)
        {
            catSpawner.SetActive(true);
            Debug.Log("🐱 CAT SPAWNER ACTIVATED!");
        }

        // Show camera UI
        if (cameraCanvas != null)
        {
            cameraCanvas.SetActive(true);
        }

        // Hide results
        if (resultBoardCanvas != null)
        {
            resultBoardCanvas.SetActive(false);
        }
    }


    // =========================================================
    // TIMER
    // =========================================================

    private void Update()
    {
        if (!timerRunning)
            return;

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            timerRunning = false;

            UpdateTimerDisplay();

            TimeUp();

            return;
        }

        UpdateTimerDisplay();
    }


    // =========================================================
    // UPDATE TIMER TEXT
    // =========================================================

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
            return;

        int minutes =
            Mathf.FloorToInt(remainingTime / 60f);

        int seconds =
            Mathf.FloorToInt(remainingTime % 60f);

        timerText.text =
            string.Format("{0:00}:{1:00}", minutes, seconds);
    }


    // =========================================================
    // TAKE PHOTO
    // =========================================================

    public void TakePhoto()
    {
        Debug.Log("📸 TAKE PHOTO BUTTON PRESSED!");

        // Don't allow photos outside the round
        if (!timerRunning)
        {
            Debug.Log("⏰ PHOTO ROUND IS NOT RUNNING!");
            return;
        }

        // Check Main Camera
        if (mainCamera == null)
        {
            Debug.LogError("❌ Main Camera is not assigned!");
            return;
        }

        // Check Render Texture
        if (photoRenderTexture == null)
        {
            Debug.LogError("❌ Photo Render Texture is not assigned!");
            return;
        }


        // =====================================================
        // CHECK PHOTO VALIDITY
        // =====================================================

        bool validPhoto = IsPhotoValid();

        photoValidity.Add(validPhoto);

        if (validPhoto)
        {
            Debug.Log(
                "📸✅ VALID PHOTO! At least one cat eye is visible."
            );
        }
        else
        {
            Debug.Log(
                "📸❌ INVALID PHOTO! No cat eye is visible."
            );
        }


        // =====================================================
        // CAPTURE IMAGE
        // =====================================================

        Texture2D newPhoto = CaptureMainCamera();

        if (newPhoto == null)
        {
            // Remove validity if capture failed
            photoValidity.RemoveAt(photoValidity.Count - 1);
            return;
        }


        // Store photo
        capturedPhotos.Add(newPhoto);

        Debug.Log(
            "📸 PHOTO " +
            capturedPhotos.Count +
            " SAVED | " +
            (validPhoto ? "VALID" : "INVALID")
        );


        // =====================================================
        // SHOW PHOTO PREVIEW
        // =====================================================

        if (photoPreview != null)
        {
            photoPreview.texture = newPhoto;
        }


        if (previewCoroutine != null)
        {
            StopCoroutine(previewCoroutine);
        }

        previewCoroutine =
            StartCoroutine(ShowPhotoPreview());
    }


    // =========================================================
    // PHOTO VALIDATION
    // =========================================================

    private bool IsPhotoValid()
    {
        CatPhotoTarget[] cats =
            FindObjectsByType<CatPhotoTarget>(
                FindObjectsSortMode.None
            );

        foreach (CatPhotoTarget cat in cats)
        {
            if (cat == null)
                continue;

            // At least ONE eye visible = valid

            if (IsEyeVisible(cat.leftEye))
            {
                return true;
            }

            if (IsEyeVisible(cat.rightEye))
            {
                return true;
            }
        }

        return false;
    }


    // =========================================================
    // CHECK EYE VISIBILITY
    // =========================================================

    private bool IsEyeVisible(Transform eye)
    {
        if (eye == null)
            return false;

        // Convert eye position to camera viewport
        Vector3 viewportPoint =
            mainCamera.WorldToViewportPoint(eye.position);

        // Eye is behind the camera
        if (viewportPoint.z <= 0f)
            return false;

        // Eye is outside camera frame
        if (viewportPoint.x < 0f ||
            viewportPoint.x > 1f ||
            viewportPoint.y < 0f ||
            viewportPoint.y > 1f)
        {
            return false;
        }

        return true;
    }


    // =========================================================
    // CAPTURE MAIN CAMERA
    // =========================================================

    private Texture2D CaptureMainCamera()
    {
        RenderTexture previousTarget =
            mainCamera.targetTexture;

        RenderTexture previousActive =
            RenderTexture.active;


        // Send Main Camera output to Render Texture
        mainCamera.targetTexture =
            photoRenderTexture;

        mainCamera.Render();


        // Read Render Texture
        RenderTexture.active =
            photoRenderTexture;


        Texture2D photo =
            new Texture2D(
                photoRenderTexture.width,
                photoRenderTexture.height,
                TextureFormat.RGB24,
                false
            );


        photo.ReadPixels(
            new Rect(
                0,
                0,
                photoRenderTexture.width,
                photoRenderTexture.height
            ),
            0,
            0
        );


        photo.Apply();


        // Restore Main Camera
        RenderTexture.active =
            previousActive;

        mainCamera.targetTexture =
            previousTarget;


        return photo;
    }


    // =========================================================
    // PHOTO PREVIEW
    // =========================================================

    private IEnumerator ShowPhotoPreview()
    {
        if (photoPreviewPanel != null)
        {
            photoPreviewPanel.SetActive(true);
        }


        yield return new WaitForSeconds(previewTime);


        if (photoPreviewPanel != null)
        {
            photoPreviewPanel.SetActive(false);
        }


        previewCoroutine = null;
    }


    // =========================================================
    // TIME UP
    // =========================================================

    private void TimeUp()
{
      Debug.Log("⏰ TIME UP!");

      Debug.Log(
        "📸 TOTAL PHOTOS: " +
        capturedPhotos.Count
      );

    // Stop cat spawning
      if (catSpawner != null)
    {
        catSpawner.SetActive(false);
    }

    // Hide camera UI
      if (cameraCanvas != null)
    {
        cameraCanvas.SetActive(false);
    }

    // Hide photo preview
      if (photoPreviewPanel != null)
    {
        photoPreviewPanel.SetActive(false);
    }

    // Show time-over message
      if (timeOverPanel != null)
    {
        timeOverPanel.SetActive(true);
    }
}

    // =========================================================
    // SHOW RESULT BOARD
    // =========================================================

    public void ShowResults()
    {
        Debug.Log("🏆 SHOWING PHOTO RESULTS");


        if (cameraCanvas != null)
        {
            cameraCanvas.SetActive(false);
        }


        if (resultBoardCanvas != null)
        {
            resultBoardCanvas.SetActive(true);
        }


        // Display captured photos
        for (int i = 0; i < resultPhotoSlots.Length; i++)
        {
            if (resultPhotoSlots[i] == null)
                continue;


            if (i < capturedPhotos.Count)
            {
                resultPhotoSlots[i].texture =
                    capturedPhotos[i];

                resultPhotoSlots[i].gameObject.SetActive(true);
            }
            else
            {
                resultPhotoSlots[i].texture = null;

                resultPhotoSlots[i].gameObject.SetActive(false);
            }
        }


        Debug.Log(
            "🏆 " +
            capturedPhotos.Count +
            " PHOTOS DISPLAYED!"
        );
    }


    // =========================================================
    // GET PHOTO COUNT
    // =========================================================

    public int GetPhotoCount()
    {
        return capturedPhotos.Count;
    }


    // =========================================================
    // GET PHOTOS
    // =========================================================

    public List<Texture2D> GetCapturedPhotos()
    {
        return capturedPhotos;
    }


    // =========================================================
    // GET PHOTO VALIDITY
    // =========================================================

    public List<bool> GetPhotoValidity()
    {
        return photoValidity;
    }


    // =========================================================
    // CLEAR PHOTOS
    // =========================================================

    public void ClearPhotos()
    {
        foreach (Texture2D photo in capturedPhotos)
        {
            if (photo != null)
            {
                Destroy(photo);
            }
        }


        capturedPhotos.Clear();

        // Clear validity data
        photoValidity.Clear();


        // Clear result slots
        if (resultPhotoSlots != null)
        {
            foreach (RawImage slot in resultPhotoSlots)
            {
                if (slot != null)
                {
                    slot.texture = null;
                    slot.gameObject.SetActive(false);
                }
            }
        }


        Debug.Log("🗑️ ALL PHOTOS CLEARED!");
    }
    // =========================================================
// CLOSE TIME OVER PANEL
// =========================================================

    public void CloseTimeOverPanel()
{
      if (timeOverPanel != null)
    {
        timeOverPanel.SetActive(false);

        Debug.Log("⏰ Time Over panel closed.");
    }
}
}