using System.Collections.Generic;
using Assets.FantasyMonsters.Scripts;
using UnityEngine;

public class GoatListing : MonoBehaviour
{
    [SerializeField] private GoatStore gameStore; // Reference to GameStore script
    [SerializeField] private float spacing = 2f; // Spacing between items
    [SerializeField] private float snapSpeed = 10f; // Speed of snapping to item
    [SerializeField] private float scaleMultiplier = 1.2f; // Scale of the selected item

    private List<Transform> itemTransforms = new List<Transform>();
    private List<float> itemWorldPositions = new List<float>();
    private int selectedItemIndex = 0;
    private bool isDragging = false;
    private Vector3 dragStartPosition;
    private Vector3 contentStartPosition;

    private Transform contentTransform;

    public Sprite statusTag;
    public GameObject coin;

    private void Start()
    {
        InitializeList();
    }

    private void InitializeList()
    {
        // Get all store items
        List<GameObject> storeItems = gameStore.GetAllStoreItems();

        // Create a parent object for content
        GameObject contentObject = new GameObject("Content");
        contentObject.transform.SetParent(transform);
        contentTransform = contentObject.transform;
        contentTransform.position = Vector3.zero;

        // Position items horizontally
        float totalWidth = 0;

        for (int i = 0; i < storeItems.Count; i++)
        {
            bool itemPurchased = gameStore.CheckItemPurchased(i);

            GameObject newItem = Instantiate(storeItems[i], contentTransform);
            newItem.name = storeItems[i].name;

            newItem.GetComponent<Monster>().SetStoreItem(gameStore, i);

                GameObject tag = new GameObject("tagg");

                tag.transform.SetParent(newItem.transform);
                SpriteRenderer sr = tag.AddComponent<SpriteRenderer>();
                sr.sprite = statusTag;
                sr.sortingOrder = 4000;
                tag.transform.localScale = new Vector3(0.1f, 0.1f, 0.05f);
                tag.transform.localPosition = new Vector3(0.27f, 1.84f, -6.45f);
            if (!itemPurchased)
            {
                tag.SetActive(false);
            }

            // Add cost text dynamically
            GameObject costText = new GameObject("CostText");
        
            costText.transform.SetParent(newItem.transform);
            TextMesh textMesh = costText.AddComponent<TextMesh>();
            textMesh.characterSize = 0.04f;
            textMesh.fontSize = 31;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.anchor = TextAnchor.MiddleLeft;
            textMesh.text = itemPurchased == false ? $"{gameStore.storeItems[i].Purchasecost}" : "";
            textMesh.color = Color.white;
            textMesh.characterSize = 0.2f;
            //textMesh.font = font; //causing issues
            costText.transform.localPosition = new Vector3(-0.65f,-1,0); // Adjust position as needed

            GameObject desText = new GameObject("kill");

            desText.transform.SetParent(newItem.transform);
            TextMesh des = desText.AddComponent<TextMesh>();
            des.characterSize = 0.1f;
            des.fontSize = 19;
            des.fontStyle = FontStyle.Bold;
            des.anchor = TextAnchor.MiddleCenter;
            des.text = "Slaughtering Makes " + $"{gameStore.storeItems[i].KillCoast}" + " Coins";
            des.color = Color.white;
            //textMesh.font = font; //causing issues
            desText.transform.localPosition = new Vector3(0, 5, 0); // Adjust position as needed
            desText.transform.localScale = new Vector3(2, 2, 2);

            GameObject clone = Instantiate(coin);
            clone.transform.SetParent(newItem.transform);
            clone.transform.localPosition = new Vector3(-0.75f, 0.55f, -3.72f);
            clone.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
            if (itemPurchased)
            {
                clone.SetActive(false);
            }

            // Position the item
            newItem.transform.localPosition = new Vector3(totalWidth, 0, 0);
            totalWidth += spacing;

            itemTransforms.Add(newItem.transform);
            itemWorldPositions.Add(newItem.transform.position.x); // Store world position
        }
    }

    private void Update()
    {
        HandleInput();
        if (isDragging) return;

        float nearestPosition = float.MaxValue;

        // Find the nearest item to the center of the screen
        for (int i = 0; i < itemWorldPositions.Count; i++)
        {
            float distance = Mathf.Abs(Camera.main.WorldToScreenPoint(itemTransforms[i].position).x - Screen.width / 2);
            if (distance < nearestPosition)
            {
                nearestPosition = distance;
                selectedItemIndex = i;
                itemTransforms[i].GetComponent<CapsuleCollider2D>().enabled = true;
            }
            else
            {
                itemTransforms[i].GetComponent<CapsuleCollider2D>().enabled = false;
            }
        }

        foreach(Transform i in itemTransforms)
        {
            if(i != itemTransforms[selectedItemIndex])
            {
                i.Find("kill").gameObject.SetActive(false);
            }
        }

        itemTransforms[selectedItemIndex].Find("kill").gameObject.SetActive(true);


        // Snap to the nearest item
        Vector3 targetPosition = new Vector3(-itemTransforms[selectedItemIndex].localPosition.x, contentTransform.localPosition.y, contentTransform.localPosition.z);
        contentTransform.localPosition = Vector3.Lerp(contentTransform.localPosition, targetPosition, snapSpeed * Time.deltaTime);

        // Scale items
        for (int i = 0; i < itemTransforms.Count; i++)
        {
            if (i == selectedItemIndex)
            {
                itemTransforms[i].localScale = Vector3.Lerp(itemTransforms[i].localScale, Vector3.one * scaleMultiplier, Time.deltaTime * snapSpeed);
            }
            else
            {
                itemTransforms[i].localScale = Vector3.Lerp(itemTransforms[i].localScale, Vector3.one, Time.deltaTime * snapSpeed);
            }
        }
    }

    private void HandleInput()
    {
        // Mouse Input
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            ContinueDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }

        // Touch Input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    StartDrag(touch.position);
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                        ContinueDrag(touch.position);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    EndDrag();
                    break;
            }
        }
    }

    private void StartDrag(Vector3 position)
    {
        isDragging = true;
        dragStartPosition = position;
        contentStartPosition = contentTransform.localPosition;
    }

    private void ContinueDrag(Vector3 position)
    {
        Vector3 dragDelta = position - dragStartPosition;
        contentTransform.localPosition = contentStartPosition + new Vector3(dragDelta.x, 0, 0) * 0.01f;
    }

    private void EndDrag()
    {
        isDragging = false;
    }

}
