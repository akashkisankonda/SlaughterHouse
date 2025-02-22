using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.FantasyMonsters.Scripts;
using TMPro;

public class Goats : MonoBehaviour
{
    //private List<GameObject> goatPrefabs; // List of input goat prefabs
    public int queueSize = 5; // Size of the queue
    public float spacing = 2.0f; // Horizontal spacing between goats
    public float moveSpeed = 3.0f; // Speed at which the goat moves to the target
    public float smoothMoveSpeed = 5.0f; // Speed of smooth repositioning

    private Queue<GameObject> goatQueue = new Queue<GameObject>();
    private Vector3 queueStartPosition;
    private GameObject currentMovingGoat = null;
    private bool isGoatAtTarget = false;
    public Transform targetX;

    public Monster butcher;

    private GoatStore goatStore;

    public TextMeshProUGUI coins;

    private TraumaInducer traumaInducer;

    private List<GameObject> bodys = new();
    public float bodydragspeed = 5f;

    private bool isGoatDead = true;

    public PingPongIterator pingPongIterator;
    public Animator bonusAnimator;

    private bool isPingPongInitialised = false;

    private bool butcherFallen = false;

    private bool disableProcessing = false;

    void Start()
    {
        pingPongIterator.gameObject.SetActive(false);
        traumaInducer = FindObjectOfType<TraumaInducer>();
        goatStore = FindObjectOfType<GoatStore>();
        coins.text = goatStore.GetTotalCoinsAvailable().ToString();

        queueStartPosition = transform.position;

        // Initialize the queue with random goats
        for (int i = 0; i < queueSize; i++)
        {
            EnqueueRandomGoat();
        }

        Debug.Log("Done !!!");

    }


    void Update()
    {
        if (disableProcessing) return;

        if(butcherFallen && (Input.GetMouseButtonDown(0) || Input.touchCount > 0))
        {
            //pick him up
            butcher.WakeUp();
            butcherFallen = false;
            isPingPongInitialised = false;
            currentMovingGoat = goatQueue.Dequeue();
            isGoatAtTarget = false;
            StartCoroutine(SmoothRepositionQueue());
            Debug.Log("isGoatAtTarget " + isGoatAtTarget + " isGoatDead " + isGoatDead + " isPingPongInitialised " + isPingPongInitialised);
            return;
        }
        // Handle user input for the first touch (move goat to target)
        if ((Input.GetMouseButtonDown(0) || Input.touchCount > 0) && isGoatDead && !isGoatAtTarget)
        {
            if (goatQueue.Count > 0)
            {
                DisposeBody();
                currentMovingGoat = goatQueue.Dequeue();
                StartCoroutine(SmoothRepositionQueue()); // Start smooth repositioning
                Debug.Log("Moving goat to target...");
            }
        }

        // Move the goat towards the target position
        if (currentMovingGoat != null && !isGoatAtTarget && !isGoatDead)
        {
            Vector3 targetPosition = new Vector3(targetX.position.x, currentMovingGoat.transform.position.y, currentMovingGoat.transform.position.z);
            currentMovingGoat.transform.position = Vector3.MoveTowards(currentMovingGoat.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            currentMovingGoat.GetComponent<Monster>().SetState(MonsterState.Walk);

            if (Vector3.Distance(currentMovingGoat.transform.position, targetPosition) < 0.1f)
            {
                isGoatAtTarget = true;
                currentMovingGoat.GetComponent<Monster>().SetState(MonsterState.Idle);
                Debug.Log("Goat reached target position.");
            }
        }

        // Handle user input for the second touch (process death)
        if (isGoatAtTarget && !isGoatDead && !isPingPongInitialised)
        {
            pingPongIterator.gameObject.SetActive(true);
            isPingPongInitialised = true;
            StoreItem animalInfo = goatStore.GetStoreItem(currentMovingGoat.name);
            Debug.Log("pingPongIterator.Initialize");
            pingPongIterator.Initialize(animalInfo.breakpoints.ToArray(), animalInfo.killRange, ProcessDeath, animalInfo.lifes);
        }

        // Move disposed bodies
        List<GameObject> bodyDispose = new();
        foreach (var item in bodys)
        {
            item.transform.position += Time.deltaTime * bodydragspeed * Vector3.left;
            if(item.name == "runninganimal")
            {
            item.GetComponent<Monster>().SetState(MonsterState.Run);
            }
            if (item.transform.position.x < -20)
            {
                bodyDispose.Add(item);
            }
        }

        foreach (var body in bodyDispose)
        {
            bodys.Remove(body);
            Destroy(body);
        }
    }

    void DisableSlider() { pingPongIterator.gameObject.SetActive(false); }

    private IEnumerator ProcessDeath(bool success, bool doubleCoins)
    {
        disableProcessing = true;
        if (success == false)
        {
            currentMovingGoat.GetComponent<Monster>().Attack();
            butcher.Die();
            butcherFallen = true;
            yield return new WaitForSeconds(1f);
            currentMovingGoat.GetComponent<Monster>().SetState(MonsterState.Run);
            currentMovingGoat.name = "runninganimal";
            DisposeBody();
            isGoatAtTarget = false;
            disableProcessing = false;
            Invoke(nameof(DisableSlider), 0.5f);
            yield break;
        }
        butcher.Attack();
        // Handle goat death
        yield return new WaitForSeconds(0.7f);
        butcher.transform.Find("SwordSlashThickRed").GetComponent<ParticleSystem>().Play();
        
        isGoatDead = true;
        isGoatAtTarget = false;

        
        Invoke(nameof(DisableSlider), 0.5f);

        if (currentMovingGoat != null)
        {
            StoreItem animalInfo = goatStore.GetStoreItem(currentMovingGoat.name);
            currentMovingGoat.GetComponent<Monster>().SetState(MonsterState.Death);
            
            currentMovingGoat.transform.Find("BloodSplatCritical2D").GetComponent<ParticleSystem>().Play();
            currentMovingGoat.transform.Find("BloodShowerLoop2D").GetComponent<ParticleSystem>().Play();
            currentMovingGoat.transform.Find("BloodPoolGrowing").GetComponent<ParticleSystem>().Play();
            traumaInducer.Impose(animalInfo.intensity);
            goatStore.AddCoins(animalInfo.KillCoast * (doubleCoins ? 2 : 1));
            bonusAnimator.SetTrigger("Play");
            GoatBlood.instance.EnableInactiveChildren(1);
            coins.text = goatStore.GetTotalCoinsAvailable().ToString();
        }
        isPingPongInitialised = false;
        disableProcessing = false;
        Debug.Log("Goat death processed.");
    }

    private void DisposeBody()
    {
        // Prepare for next goat
        isGoatDead = false;

        if (currentMovingGoat != null)
        {
            bodys.Add(currentMovingGoat);
            EnqueueRandomGoat();
        }
    }


    private void EnqueueRandomGoat()
    {
        List<GameObject> gs = goatStore.GetPurchasedItems();
        if (gs.Count == 0) return;

        GameObject randomGoat = Instantiate(gs[Random.Range(0, gs.Count)]);
        randomGoat.GetComponent<CapsuleCollider2D>().enabled = false;
        EnqueueGoat(randomGoat);
    }

    private void EnqueueGoat(GameObject goat)
    {
        goatQueue.Enqueue(goat);
        goat.transform.SetParent(transform); // Optional: Make the queue manager the parent
        ImmediatePositionQueue(goat);
        StartCoroutine(SmoothRepositionQueue()); // Reposition queue smoothly whenever a goat is added
    }

    private IEnumerator SmoothRepositionQueue()
    {
        Vector3 currentPosition = queueStartPosition;

        // Dynamically calculate target positions for each goat in the queue
        List<Vector3> targetPositions = new List<Vector3>();
        List<float> randomisedSpeed = new();
        foreach (GameObject goat in goatQueue)
        {
            targetPositions.Add(currentPosition);
            randomisedSpeed.Add(Random.Range(smoothMoveSpeed, smoothMoveSpeed + 2.0f));
            currentPosition.x += spacing;
        }

        bool allAtTarget = false;

        while (!allAtTarget)
        {
            allAtTarget = true;
            int index = 0;

            // Iterate over goats and move them smoothly
            foreach (GameObject goat in goatQueue)
            {
                if (index < targetPositions.Count)
                {
                    if (Vector3.Distance(goat.transform.position, targetPositions[index]) > 0.1f)
                    {
                        allAtTarget = false;
                        goat.transform.position = Vector3.Lerp(goat.transform.position, targetPositions[index], randomisedSpeed[index] * Time.deltaTime);
                        goat.GetComponent<Monster>().SetState(MonsterState.Walk);
                    }
                    else
                    {
                        goat.GetComponent<Monster>().SetState(MonsterState.Idle);
                    }
                }

                index++;
            }

            yield return null; // Wait for the next frame
        }
    }


    private void ImmediatePositionQueue(GameObject obj)
    {
        Vector3 currentPosition = queueStartPosition;

        // Store initial positions for smooth movement
        List<Vector3> targetPositions = new List<Vector3>();
        foreach (GameObject goat in goatQueue)
        {
            targetPositions.Add(currentPosition);
            currentPosition.x += spacing;
        }

        Vector3 cp = obj.transform.position;
        cp.x = currentPosition.x;
        obj.transform.position = cp;
    }
}
