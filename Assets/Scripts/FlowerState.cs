using UnityEngine;

public class FlowerState : MonoBehaviour
{
    [Header("State")]
    public bool isFull; 

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetVisualState(bool full)
    {
        isFull = full;
        if (animator != null)
        {
            // Ensure your Flower Animator has a bool named "HasPollen"
            animator.SetBool("HasPollen", isFull);
        }
    }
}