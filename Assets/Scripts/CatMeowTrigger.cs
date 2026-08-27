using UnityEngine;

public class CatMeowTrigger : MonoBehaviour
{
    [Header("Player")]
    public Transform playerCamera;

    [Header("Cat")]
    public AudioSource meowAudio;
    public GameObject dialogueCanvas;

    [Header("Distance")]
    public float activationDistance = 3f;

    private bool hasPlayed = false;

    private void Start()
    {
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
    }

    private void Update()
    {
        if (hasPlayed || playerCamera == null)
            return;

        float distance = Vector3.Distance(
            playerCamera.position,
            transform.position
        );

        if (distance <= activationDistance)
        {
            ActivateCat();
        }
    }

    private void ActivateCat()
    {
        hasPlayed = true;

        if (meowAudio != null)
            meowAudio.Play();

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);
    }
}