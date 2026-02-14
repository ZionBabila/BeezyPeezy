using UnityEngine;

public class SimpleMover : MonoBehaviour
{
    [Tooltip("Movement speed downwards")]
    [SerializeField] private float speed = 5f;

    void Update()
    {
        // Move the object down every frame
        transform.Translate(Vector3.down * speed * Time.deltaTime);
    }
}