using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoatSwitch : MonoBehaviour
{
    public List<GameObject> gameSceneElements;
    public List<GameObject> shopSceneElements;

    private void Start()
    {
        ToggleGameScene();
        ToggleShopScene();
    }

    private bool gameSceneStatus = false;
    public void ToggleGameScene()
    {
        gameSceneStatus = !gameSceneStatus;
        foreach (var item in gameSceneElements)
        {
            item.SetActive(gameSceneStatus);
        }
    }

    private bool shopSceneStatus = true;
    public void ToggleShopScene()
    {
        shopSceneStatus = !shopSceneStatus;
        foreach (var item in shopSceneElements)
        {
            item.SetActive(shopSceneStatus);
        }
    }
}
