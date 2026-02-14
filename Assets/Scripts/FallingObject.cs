using UnityEngine;

public class FallingObject : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float destroyY = -6f; // הגובה שבו האובייקט נמחק (מתחת למסך)

    void Update()
    {
        // תנועה למטה על ציר Y
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // בדיקה אם יצא מהמסך
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}