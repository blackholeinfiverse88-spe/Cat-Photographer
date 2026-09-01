using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CatPhotographySystem : MonoBehaviour
{
    public static CatPhotographySystem Instance;

    // =========================================================
    // CAMERA
    // =========================================================

    [Header("PHOTO CAMERA")]
    public Camera photoCamera;

    public RenderTexture photoRenderTexture;

    // =========================================================
    // CAMERA UI
    // =========================================================

    [Header("CAMERA UI")]
    public GameObject cameraCanvas;

    // =========================================================
    // CAPTURED PHOTO
    // =========================================================

    [Header("CAPTURED PHOTO")]
    public GameObject capturedPhotoPanel;

    public RawImage capturedPhotoPreview;

    // =========================================================
    // RESULT BOARD
    // =========================================================

    [Header("CAFÉ RESULT BOARD")]

    public RawImage[] resultPhotoSlots;

    public TMP_Text[] resultScoreTexts;

    public TMP_Text totalScoreText;

    // =========================================================
    // CAT
    // =========================================================

    [Header("CAT SETTINGS")]

    public string catTag = "Cat";

    public CatSpawner catSpawner;

    // =========================================================
    // SCORING
    // =========================================================

    [Header("SCORING")]

    public int maximumPhotoScore = 100;

    public int angleWeight = 30;

    public int visibilityWeight = 40;

    public int clarityWeight = 30;

    // =========================================================
    // PHOTO DATA
    // =========================================================

    [System.Serializable]
    public class PhotoData
    {
        public Texture2D image;

        public int angleScore;

        public int visibilityScore;

        public int clarityScore;

        public int totalScore;
    }

    public List<PhotoData> capturedPhotos = new List<PhotoData>();

    // =========================================================
    // INTERNAL
    // =========================================================

    private GameObject photographedCat;

    private Texture2D currentPhoto;

    // =========================================================
    // START
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (capturedPhotoPanel != null)
        {
            capturedPhotoPanel.SetActive(false);
        }

        UpdateTotalScore();
    }

    // =========================================================
    // TAKE PHOTO
    // =========================================================

    public void TakePhoto()
    {
        Debug.Log("📸 CLICK!");

        photographedCat = FindVisibleCat();

        if (photographedCat == null)
        {
            Debug.Log("❌ No cat visible!");

            return;
        }

        CaptureImage();

        CalculatePhotoScore();

        ShowCapturedPhoto();

        Debug.Log("📸 CAT PHOTO CAPTURED!");

        Debug.Log(
            "⭐ Photo Score: " +
            capturedPhotos[capturedPhotos.Count - 1].totalScore
        );
    }

    // =========================================================
    // FIND CAT
    // =========================================================

    private GameObject FindVisibleCat()
    {
        GameObject[] cats = GameObject.FindGameObjectsWithTag(catTag);

        GameObject closestCat = null;

        float closestDistance = Mathf.Infinity;

        foreach (GameObject cat in cats)
        {
            if (!cat.activeInHierarchy)
                continue;

            Renderer catRenderer =
                cat.GetComponentInChildren<Renderer>();

            if (catRenderer == null)
                continue;

            Vector3 catPosition =
                catRenderer.bounds.center;

            Vector3 viewportPosition =
                photoCamera.WorldToViewportPoint(catPosition);

            // Cat must be in front of camera
            if (viewportPosition.z <= 0)
                continue;

            // Cat must be inside camera view
            if (viewportPosition.x < 0 ||
                viewportPosition.x > 1 ||
                viewportPosition.y < 0 ||
                viewportPosition.y > 1)
            {
                continue;
            }

            float distance =
                Vector3.Distance(
                    photoCamera.transform.position,
                    catPosition
                );

            if (distance < closestDistance)
            {
                closestDistance = distance;

                closestCat = cat;
            }
        }

        return closestCat;
    }

    // =========================================================
    // CAPTURE IMAGE
    // =========================================================

    private void CaptureImage()
    {
        if (photoRenderTexture == null)
        {
            Debug.LogError(
                "❌ Photo Render Texture is missing!"
            );

            return;
        }

        currentPhoto = new Texture2D(
            photoRenderTexture.width,
            photoRenderTexture.height,
            TextureFormat.RGB24,
            false
        );

        RenderTexture previousTexture =
            RenderTexture.active;

        RenderTexture.active =
            photoRenderTexture;

        currentPhoto.ReadPixels(
            new Rect(
                0,
                0,
                photoRenderTexture.width,
                photoRenderTexture.height
            ),
            0,
            0
        );

        currentPhoto.Apply();

        RenderTexture.active =
            previousTexture;
    }

    // =========================================================
    // CALCULATE PHOTO SCORE
    // =========================================================

    private void CalculatePhotoScore()
    {
        if (photographedCat == null)
            return;

        Renderer catRenderer =
            photographedCat.GetComponentInChildren<Renderer>();

        if (catRenderer == null)
            return;

        // -----------------------------------------------------
        // DISTANCE / CLARITY
        // -----------------------------------------------------

        float distance =
            Vector3.Distance(
                photoCamera.transform.position,
                catRenderer.bounds.center
            );

        float clarity = 1f;

        if (distance <= 3f)
        {
            clarity = 1f;
        }
        else if (distance >= 4f)
        {
            clarity = 0.6f;
        }
        else
        {
            clarity =
                Mathf.Lerp(
                    1f,
                    0.6f,
                    (distance - 3f)
                );
        }

        int clarityScore =
            Mathf.RoundToInt(
                clarity * clarityWeight
            );

        // -----------------------------------------------------
        // CENTER / ANGLE
        // -----------------------------------------------------

        Vector3 viewportPosition =
            photoCamera.WorldToViewportPoint(
                catRenderer.bounds.center
            );

        float centerDistance =
            Vector2.Distance(
                new Vector2(
                    viewportPosition.x,
                    viewportPosition.y
                ),
                new Vector2(
                    0.5f,
                    0.5f
                )
            );

        float centerScore =
            Mathf.Clamp01(
                1f -
                centerDistance * 2f
            );

        int angleScore =
            Mathf.RoundToInt(
                centerScore * angleWeight
            );

        // -----------------------------------------------------
        // VISIBILITY
        // -----------------------------------------------------

        float visibleAmount =
            CalculateVisibility(catRenderer);

        int visibilityScore =
            Mathf.RoundToInt(
                visibleAmount *
                visibilityWeight
            );

        // -----------------------------------------------------
        // TOTAL
        // -----------------------------------------------------

        int total =
            angleScore +
            visibilityScore +
            clarityScore;

        PhotoData photo =
            new PhotoData();

        photo.image = currentPhoto;

        photo.angleScore =
            angleScore;

        photo.visibilityScore =
            visibilityScore;

        photo.clarityScore =
            clarityScore;

        photo.totalScore =
            total;

        capturedPhotos.Add(photo);
    }

    // =========================================================
    // VISIBILITY
    // =========================================================

    private float CalculateVisibility(
        Renderer catRenderer
    )
    {
        Bounds bounds =
            catRenderer.bounds;

        Vector3[] points =
        {
            bounds.center,

            bounds.min,

            bounds.max,

            new Vector3(
                bounds.min.x,
                bounds.min.y,
                bounds.max.z
            ),

            new Vector3(
                bounds.min.x,
                bounds.max.y,
                bounds.min.z
            ),

            new Vector3(
                bounds.max.x,
                bounds.min.y,
                bounds.min.z
            )
        };

        int visiblePoints = 0;

        foreach (Vector3 point in points)
        {
            Vector3 viewport =
                photoCamera.WorldToViewportPoint(
                    point
                );

            if (viewport.z <= 0)
                continue;

            if (viewport.x < 0 ||
                viewport.x > 1 ||
                viewport.y < 0 ||
                viewport.y > 1)
                continue;

            visiblePoints++;
        }

        return (float)visiblePoints /
               points.Length;
    }

    // =========================================================
    // SHOW CAPTURED PHOTO
    // =========================================================

    private void ShowCapturedPhoto()
    {
        if (capturedPhotoPreview != null)
        {
            capturedPhotoPreview.texture =
                currentPhoto;
        }

        if (capturedPhotoPanel != null)
        {
            capturedPhotoPanel.SetActive(true);
        }
    }

    // =========================================================
    // RETAKE
    // =========================================================

    public void RetakePhoto()
    {
        photographedCat = null;

        if (capturedPhotoPanel != null)
        {
            capturedPhotoPanel.SetActive(false);
        }
    }

    // =========================================================
    // SUBMIT PHOTO
    // =========================================================

    public void SubmitPhoto()
    {
        if (photographedCat == null)
        {
            Debug.Log(
                "No cat photo to submit."
            );

            return;
        }

        Debug.Log("📸 PHOTO SUBMITTED!");

        // Disable the cat
        if (catSpawner != null)
        {
            catSpawner.DisableCurrentCat();
        }

        photographedCat = null;

        if (capturedPhotoPanel != null)
        {
            capturedPhotoPanel.SetActive(false);
        }

        UpdateTotalScore();
    }

    // =========================================================
    // TOTAL SCORE
    // =========================================================

    public void UpdateTotalScore()
    {
        int total = 0;

        foreach (PhotoData photo in capturedPhotos)
        {
            total += photo.totalScore;
        }

        if (totalScoreText != null)
        {
            totalScoreText.text =
                "TOTAL SCORE: " +
                total.ToString();
        }

        Debug.Log(
            "🏆 TOTAL SCORE: " +
            total
        );
    }

    // =========================================================
    // DISPLAY RESULT BOARD
    // =========================================================

    public void DisplayResultBoard()
    {
        // Clear all slots first
        for (int i = 0;
             i < resultPhotoSlots.Length;
             i++)
        {
            if (resultPhotoSlots[i] != null)
            {
                resultPhotoSlots[i].texture =
                    null;
            }

            if (i < resultScoreTexts.Length &&
                resultScoreTexts[i] != null)
            {
                resultScoreTexts[i].text =
                    "";
            }
        }

        // Display captured photos
        for (int i = 0;
             i < capturedPhotos.Count &&
             i < resultPhotoSlots.Length;
             i++)
        {
            if (resultPhotoSlots[i] != null)
            {
                resultPhotoSlots[i].texture =
                    capturedPhotos[i].image;
            }

            if (i < resultScoreTexts.Length &&
                resultScoreTexts[i] != null)
            {
                resultScoreTexts[i].text =
                    capturedPhotos[i].totalScore +
                    " ⭐";
            }
        }

        UpdateTotalScore();
    }

    // =========================================================
    // CLEAR SESSION
    // =========================================================

    public void ClearAllPhotos()
    {
        capturedPhotos.Clear();

        UpdateTotalScore();

        Debug.Log(
            "All photos cleared."
        );
    }
}