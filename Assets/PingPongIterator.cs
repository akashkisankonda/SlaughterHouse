using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
// Represents a custom object with a breakpoint and break time
public class Breakpoint
{
    public float position; // Breakpoint position on the iterator
    public float pauseDuration; // Duration to pause at the breakpoint
}

[Serializable]
// Represents the range for kill evaluation
public class KillRange
{
    public float min;
    public float max;
}
public class PingPongIterator : MonoBehaviour
{

    public delegate IEnumerator KillAction(bool success, bool doubleCoins);

    private Breakpoint[] breakpoints; // Array of breakpoints
    private KillRange killRange; // Range for kill evaluation
    private KillAction onKillAttempt; // Delegate to invoke on kill attempt

    private float iterator = 0f; // Iterator value that moves between 0 and 1
    private float iteratorSpeed = 1f; // Speed of the iterator's movement
    private bool isPaused = false; // Whether the iterator is paused

    private float pauseTimer = 0f; // Time remaining for the current pause
    private int currentBreakpointIndex = 0; // Current index in the breakpoints array
    private int breakpointDirection = 1; // Direction of traversal through breakpoints (1 = forward, -1 = backward)
    private bool isStopped = true; // Whether the iterator is stopped

    private Slider slider;
    public Image handle;
    public GoatLifesHandler goatLifesHandler;
    private int lifes;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    /// <summary>
    /// Initializes the iterator with a set of breakpoints, a kill range, and a kill delegate.
    /// </summary>
    public void Initialize(Breakpoint[] breakpoints, KillRange killRange, KillAction onKillAttempt,int life, float speed = 1)
    {
        if (!isStopped) { Debug.LogError("Previous Process Still Running"); return; }
        Reset();
        this.breakpoints = breakpoints;
        this.killRange = killRange;
        this.onKillAttempt = onKillAttempt;
        iteratorSpeed = speed;
        isStopped = false;
        lifes = life;
    }

    /// <summary>
    /// Resets the iterator and all operational variables to their default values.
    /// </summary>
    private void Reset()
    {
        iterator = 0f;
        iteratorSpeed = 1f;
        isPaused = false;
        pauseTimer = 0f;
        currentBreakpointIndex = 0;
        breakpointDirection = 1;
        breakpoints = null;
        killRange = null;
        onKillAttempt = null;

        clickCooldown = 0;
        inRangeIterator = -1;
        killAttempts = 0;
        inRangeIteratorBlocker = false;
        handle.color = Color.white;
    }

    private float clickCooldown = 0;
    private int inRangeIterator = -1;
    private int killAttempts = 0;
    private bool inRangeIteratorBlocker;
    private void Update()
    {
        if (isStopped) return;

        // Handle pause logic
        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0)
            {
                isPaused = false;
            }
            return;
        }

        // Move the iterator in a ping-pong pattern
        iterator = Mathf.PingPong(Time.time * iteratorSpeed, 1f);
        slider.value = iterator;
        if (isInKillRange() && !inRangeIteratorBlocker)
        {
            inRangeIteratorBlocker = true;
            handle.color = Color.red;
            if(inRangeIterator > 0)
            {
                inRangeIterator++;
            }
        }
        else if(!isInKillRange() && inRangeIteratorBlocker)
        {
            inRangeIteratorBlocker = false;
            handle.color = Color.white;
        }

        // Handle breakpoints
        if (breakpoints != null && breakpoints.Length > 0)
        {
            Breakpoint currentBreakpoint = breakpoints[currentBreakpointIndex];
            float previousIterator = iterator - (Time.deltaTime * iteratorSpeed);
            if ((previousIterator < currentBreakpoint.position && iterator >= currentBreakpoint.position) ||
                (previousIterator > currentBreakpoint.position && iterator <= currentBreakpoint.position))
            {
                isPaused = true;
                pauseTimer = currentBreakpoint.pauseDuration;

                // Update breakpoint index with ping-pong behavior
                currentBreakpointIndex += breakpointDirection;

                if (currentBreakpointIndex >= breakpoints.Length)
                {
                    breakpointDirection = -1; // Reverse direction to move backward
                    currentBreakpointIndex = breakpoints.Length - 2; // Step back to the second-to-last index
                }
                else if (currentBreakpointIndex < 0)
                {
                    breakpointDirection = 1; // Reverse direction to move forward
                    currentBreakpointIndex = 1; // Step forward to the second index
                }
            }
        }

        // Handle user input for kill action
        if ((Input.GetMouseButtonDown(0) || Input.touchCount > 0) && Time.time > clickCooldown)
        {
            clickCooldown = Time.time + 0.3f;
            if (isInKillRange())
            {
                killAttempts++;
                goatLifesHandler.AddLife();
                lifes--;
                if(killAttempts == 1) inRangeIterator = 1;
                if (lifes == 0)
                {
                    isStopped = true;
                    StartCoroutine(onKillAttempt?.Invoke(true, (killAttempts == inRangeIterator && killAttempts > 1)));
                }
            }
            else
            {
                isStopped = true;
                StartCoroutine(onKillAttempt?.Invoke(false, false));
            }
        }
    }

    private bool isInKillRange()
    {
        return (killRange != null && iterator >= killRange.min && iterator <= killRange.max);
    }
}
