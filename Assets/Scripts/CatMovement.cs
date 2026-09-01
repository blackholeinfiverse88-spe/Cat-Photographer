using UnityEngine;

public class CatMovement : MonoBehaviour
{
    public float speed = 1.5f;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}