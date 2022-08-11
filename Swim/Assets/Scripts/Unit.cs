using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum Status{
        Idle, Swim, Eat, Death, Dive, Init
    }
    
    [SerializeField] private AudioSource _splashSFX;

    [SerializeField] private float _swimSpeed;
    [SerializeField] private float _idleRotateSpeed;
    [SerializeField] private Feed _target;
    
    public Status CurrentStatus
    {
        get { return _status; }
        set { _lastStatus = _status; _status = value; return;}
    }
    [SerializeField] private Status _status;
    private Status _lastStatus;
    private bool _isFlipX;

    [SerializeField] private float _hungry = 100;
    [SerializeField] private float _maxHungry = 100;
    [SerializeField] private float _hungrySpeed;
    [SerializeField] private float _eatSpeed = 1;
    [SerializeField] private float _breedHungry = 70;
    [SerializeField] private float _breedChance = 1;
    [SerializeField] private float _breedTime;

    [SerializeField] private float _smokeForce;
    [SerializeField] private Vector2 _force;
    [SerializeField] private Vector2 _diveForce;

    private Rigidbody2D _rigid;
    private Animator _anim;
    private Collider2D _coll;
    private SpriteRenderer _renderer;
    private Vector2 _move;
    private float _t;

    private void Awake(){
        _rigid = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _coll = GetComponent<Collider2D>();
        _renderer = GetComponent<SpriteRenderer>();
    }
    private void Update(){
        switch(CurrentStatus){
            case Status.Dive:
                if (_lastStatus != Status.Dive) {
                    Debug.Log("Dive");
                    _anim.SetInteger("Status", (int)Status.Idle);
                    _anim.SetInteger("Spr", Random.Range(0, 5));
                    transform.position = new Vector3(Random.Range(BoardManager.Instance.Border.xMin, BoardManager.Instance.Border.xMax), BoardManager.Instance.Border.yMax + 2);
                    _isFlipX = Random.Range(0, 2) == 0;
                    transform.localScale = new Vector3(_isFlipX ? -1 : 1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);
                    _move = Vector2.down * Random.Range(_diveForce.x, _diveForce.y);
                    _rigid.velocity = _move;
                    _lastStatus = Status.Dive;
                    _t = 0;
                    _coll.enabled = false;
                    float rnd = Random.Range(0.6f, 1);
                    _splashSFX.pitch = rnd;
                    _splashSFX.volume = rnd;
                    _splashSFX.Play();
                }
                _t += Time.deltaTime * 0.5f;
                _rigid.velocity = Vector2.Lerp(_move, Vector2.zero, _t);
                if (_t >= 1)
                    CurrentStatus = Status.Idle;
                transform.Rotate((_isFlipX ? -1 : 1) * Vector3.forward * Time.deltaTime * _idleRotateSpeed);
            break;
            case Status.Idle:
                if (_lastStatus != Status.Idle) {
                    Debug.Log("Idle");
                    _anim.SetInteger("Status", (int)Status.Idle);
                    _anim.SetInteger("Spr", Random.Range(0, 5));
                    _lastStatus = Status.Idle;
                    _isFlipX = Random.Range(0, 2) == 0;
                    transform.localScale = new Vector3(_isFlipX ? -1 : 1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);
                    _move = Random.insideUnitCircle * Random.Range(_force.x, _force.y);
                    _rigid.velocity = _move;
                }
                if (_rigid.velocity == Vector2.zero) _rigid.velocity = _move;
                if (_breedTime <= 0) _coll.enabled = true;
                if (_breedTime <= 0 && BoardManager.Instance.Feeds.Count > 0) {
                    Feed closet = BoardManager.Instance.Feeds[0];
                    for (int i = 1; i < BoardManager.Instance.Feeds.Count; i++) {
                        Feed temp = BoardManager.Instance.Feeds[i];
                        if ((transform.position - closet.transform.position).sqrMagnitude > (transform.position - temp.transform.position).sqrMagnitude)
                            closet = temp;
                    }
                    _target = closet;
                    CurrentStatus = Status.Swim;
                }
                transform.Rotate((_isFlipX ? -1 : 1) * Vector3.forward * Time.deltaTime * _idleRotateSpeed);

                _hungry -= _hungrySpeed * Time.deltaTime;
                if(_breedTime > 0) _breedTime -= Time.deltaTime;
                if (_hungry < 0) _hungry = 0;
            break;
            case Status.Swim:
                if (_lastStatus != Status.Swim) {
                    //Debug.Log("Swim");
                    _anim.SetInteger("Status", (int)Status.Swim);
                    _anim.SetInteger("Spr", Random.Range(0, 2));
                    _lastStatus = Status.Swim;
                    if (_isFlipX) {
                        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                    }
                }
                if (BoardManager.Instance.Feeds.Count > 0)
                {
                    Feed closet = BoardManager.Instance.Feeds[0];
                    for (int i = 1; i < BoardManager.Instance.Feeds.Count; i++) {
                        Feed temp = BoardManager.Instance.Feeds[i];
                        if ((transform.position - closet.transform.position).sqrMagnitude > (transform.position - temp.transform.position).sqrMagnitude)
                            closet = temp;
                    }
                    _target = closet;
                }
                if (!_target.gameObject.activeSelf){
                    CurrentStatus = Status.Idle;
                    break;
                } else if ((_target.transform.position - transform.position).sqrMagnitude < 0.1f) {
                    CurrentStatus = Unit.Status.Eat;
                    _target.Hold();
                    break;
                }
                
                Vector3 dir = (_target.transform.position - transform.position).normalized;
                float angle = Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle-90, Vector3.forward);
                _rigid.velocity = dir * _swimSpeed;

                _hungry -= _hungrySpeed * Time.deltaTime;
                if (_hungry < 0) _hungry = 0;
            break;
            case Status.Eat:
                if (_lastStatus != Status.Eat) {
                    //Debug.Log("Eat");
                    _anim.SetInteger("Status", (int)Status.Eat);
                    _isFlipX = (transform.position.x - _target.transform.position.x) < 0;
                    transform.localScale = new Vector3(_isFlipX ? -1 : 1 * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                    _lastStatus = Status.Eat;
                }
                transform.rotation = Quaternion.identity;
                _rigid.velocity = Vector3.zero;
                if (_target != null && !_target.gameObject.activeSelf) {
                    CurrentStatus = Status.Idle;
                    break;;
                }
                if (_hungry > _breedHungry && (BoardManager.Instance.UnitCount == 1 || Random.Range(0f, 1f) < _breedChance)) {
                    //Debug.Log("Breed");
                    _hungry -= _breedHungry;
                    Unit unit = BoardManager.Instance.SpawnUnit();
                    unit.Init();
                    unit.transform.position = transform.position;
                    unit._breedTime = 2f;
                    unit._coll.enabled = false;
                    unit._anim.SetInteger("Status", (int)Status.Idle);
                    unit._anim.SetInteger("Spr", Random.Range(0, 5));
                    unit.CurrentStatus = Status.Idle;
                    unit.gameObject.SetActive(true);
                } else if (_hungry > _breedHungry){
                    _hungry -= _breedHungry;
                    if (BoardManager.Instance.UnitCount != 1)
                        Kill();
                }
                _target.Eat(_eatSpeed * Time.deltaTime);
                _hungry += _eatSpeed * Time.deltaTime;
                if (_hungry > _maxHungry) _hungry = _maxHungry;
            break;
            case Status.Death:
            if (_lastStatus != Status.Death) {
                _anim.SetInteger("Status", (int)Status.Idle);
                _anim.SetInteger("Spr", Random.Range(0, 5));
                _anim.SetBool("IsDeath", true);
                _isFlipX = Random.Range(0, 2) == 0;
                transform.localScale = new Vector3(_isFlipX ? -1 : 1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);
                _move = Random.insideUnitCircle * Random.Range(_force.x, _force.y);
                _rigid.velocity = _move;
                _lastStatus = Status.Death;
            }
            transform.Rotate((_isFlipX ? -1 : 1) * Vector3.forward * Time.deltaTime * _idleRotateSpeed);
            break;
        }

        if (transform.position.y <= BoardManager.Instance.SmokeLine) {
            _rigid.velocity = new Vector2(_rigid.velocity.x, -_smokeForce);
            Kill();
        }
    }

    public void Kill(){
        CurrentStatus = Status.Death;
        BoardManager.Instance.DestroyUnit(this);
    }

    public void OnDeath(){
        BoardManager.Instance.UnitPool.EnqueueObjectPool(gameObject);
    }
    
    public void Init(){
        CurrentStatus = Status.Init;
        _lastStatus = Status.Init;
        _isFlipX = false;
        _breedTime = 0;
        _anim.SetBool("IsDeath", false);
    }

    public void DiveSpawn(){
        CurrentStatus = Status.Dive;
    }
}
