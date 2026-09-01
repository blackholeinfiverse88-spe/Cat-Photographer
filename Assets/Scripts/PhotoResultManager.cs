using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PhotoResultManager : MonoBehaviour
{
    [Header("PHOTO CAPTURE")]
    public PhotoCapture photoCapture;

    [Header("RESULT TEXT")]
    public TMP_Text resultText;

    [Header("SCORING")]
    public int pointsPerValidPhoto = 100;


    // =========================================================
    // SHOW RESULTS
    // =========================================================

    public void ShowPhotoResults()
    {
        if (photoCapture == null)
        {
            Debug.LogError("❌ PhotoCapture is not assigned!");
            return;
        }


        List<bool> photoValidity =
            photoCapture.GetPhotoValidity();


        int totalPhotos = photoValidity.Count;
        int validPhotos = 0;
        int invalidPhotos = 0;
        int totalScore = 0;


        // Calculate score
        foreach (bool valid in photoValidity)
        {
            if (valid)
            {
                validPhotos++;

                totalScore += pointsPerValidPhoto;
            }
            else
            {
                invalidPhotos++;
            }
        }


        // Display result
        if (resultText != null)
        {
            resultText.text =
                "📸 PHOTO RESULTS\n\n" +

                "PHOTOS TAKEN: " +
                totalPhotos +

                "\n\nVALID PHOTOS: " +
                validPhotos +

                "\n\nINVALID PHOTOS: " +
                invalidPhotos +

                "\n\n🏆 TOTAL SCORE: " +
                totalScore;
        }


        Debug.Log(
            "🏆 PHOTO RESULTS\n" +
            "Photos: " + totalPhotos +
            "\nValid: " + validPhotos +
            "\nInvalid: " + invalidPhotos +
            "\nScore: " + totalScore
        );
    }
}