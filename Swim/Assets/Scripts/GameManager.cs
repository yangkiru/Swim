using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Camera _mainCam;
    
    private void Awake(){
        Instance = this;
        _mainCam = Camera.main;
    }

    private void Start() {
        BoardManager.Instance.Init();
    }

    private void Update(){
        if(Input.GetMouseButtonUp(0)) {
            Feed feed = BoardManager.Instance.SpawnFeed();
            Vector3 pos = _mainCam.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;
            feed.transform.position = pos;
            feed.gameObject.SetActive(true);
        }
    }
}
