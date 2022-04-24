using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnNotification : MonoBehaviour
{
    [Tooltip("The duration the notification appears for")]
    public float Duration;

    private Animator animator;
    private float time = -1f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void LateUpdate()
    {
        animator.SetBool("Show", false);
        animator.SetFloat("Time", time);

        time = Mathf.Clamp(time - Time.deltaTime, -1f, Duration);
    }

    /// <summary>
    /// Shows the notification animation
    /// </summary>
    public void Play()
    {
        time = Duration;

        animator.SetBool("Show", true);
        animator.SetFloat("Time", time);
    }

    /// <summary>
    /// Hides the notification
    /// </summary>
    public void Stop()
    {
        time = -1;
    }
}
