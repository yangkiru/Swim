using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    public Rect Border;
    public float SmokeLine;
    public BoxCollider2D[] Walls;

    public int UnitCount {get {return _units.Count;}}
    [SerializeField] private List<Unit> _units;
    public List<Feed> Feeds {get{return _feeds;}}
    [SerializeField] private List<Feed> _feeds;
    [SerializeField] private GameObject _unitPrefab;
    public ObjectPool UnitPool {get{return _unitPool;}}
    [SerializeField] private ObjectPool _unitPool;
    [SerializeField] private GameObject _feedPrefab;
    public ObjectPool FeedPool {get{return _feedPool;}}
    [SerializeField] private ObjectPool _feedPool;
    private IEnumerator InitCoroutine;
    
    private void Awake(){
        Instance = this;
        for(int i = 0; i < 30; i++){
            _unitPool.EnqueueObjectPool(Instantiate(_unitPrefab));
            _feedPool.EnqueueObjectPool(Instantiate(_feedPrefab));
        }
        Walls[0].offset = new Vector2(0, Border.yMax+0.5f);
        Walls[0].size = new Vector2(Border.width, 1);
        Walls[1].offset = new Vector2(Border.xMin-0.5f, 0);
        Walls[1].size = new Vector2(1, Border.height);
        Walls[2].offset = new Vector2(Border.xMax+0.5f, 0);
        Walls[2].size = new Vector2(1, Border.height);
    }

    public void Init(){
        for(int i = 0; i < _units.Count; i++) {
            _unitPool.EnqueueObjectPool(_units[i].gameObject);
        }
        for(int i = 0; i < _feeds.Count; i++) {
            _feedPool.EnqueueObjectPool(_feeds[i].gameObject);
        }
        _unitPool.Clear();
        _feedPool.Clear();
        if (InitCoroutine != null) StopCoroutine(InitCoroutine);
        InitCoroutine = InitCorou();
        StartCoroutine(InitCoroutine);
    }

    public Unit SpawnUnit(){
        Unit unit = _unitPool.DequeueObjectPool().GetComponent<Unit>();
        _units.Add(unit);
        return unit;
    }

    public Feed SpawnFeed(){
        Feed feed = _feedPool.DequeueObjectPool().GetComponent<Feed>();
        _feeds.Add(feed);
        feed.Init();
        return feed;
    }

    public void DestroyFeed(Feed feed){
        _feeds.Remove(feed);
        _feedPool.EnqueueObjectPool(feed.gameObject);
    }

    public void DestroyUnit(Unit unit, bool isEnqueue = false){
        _units.Remove(unit);
        if (isEnqueue) _unitPool.EnqueueObjectPool(unit.gameObject);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(Border.xMin, Border.yMin, 0), new Vector3(Border.xMin, Border.yMax, 0));
        Gizmos.DrawLine(new Vector3(Border.xMin, Border.yMax, 0), new Vector3(Border.xMax, Border.yMax, 0));
        Gizmos.DrawLine(new Vector3(Border.xMax, Border.yMin, 0), new Vector3(Border.xMax, Border.yMax, 0));
        Gizmos.DrawLine(new Vector3(Border.xMin, Border.yMin, 0), new Vector3(Border.xMax, Border.yMin, 0));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(Border.xMin, SmokeLine, 0), new Vector3(Border.xMax, SmokeLine, 0));
    }

    private IEnumerator InitCorou() {
        float t;
        for(int i = 0; i < 5; i++) {
            Unit unit = SpawnUnit();
            unit.Init();
            unit.DiveSpawn();
            unit.gameObject.SetActive(true);
            t = Random.Range(0.5f, 1);
            do {
                yield return null;
                t -= Time.deltaTime;
            } while(t > 0);
        }
        while(true) {
            t = Random.Range(5f, 15f);
            do {
                yield return null;
                t -= Time.deltaTime;
            } while(t > 0);
            int rnd = Random.Range(0, 5);
            for (int i = 0; i < rnd; i++){
                Unit unit = SpawnUnit();
                unit.Init();
                unit.DiveSpawn();
                unit.gameObject.SetActive(true);
                t = Random.Range(0.5f, 1f);
                do {
                    yield return null;
                    t -= Time.deltaTime;
                } while(t > 0);
            }
        }
    }
}
