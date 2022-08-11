using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feed : MonoBehaviour
{
    [SerializeField] private AudioSource _popSFX;
    [SerializeField] private float _speed;
    [SerializeField] private bool _isHold;
    [SerializeField] private float _hp = 1000;

    [SerializeField] private float _maxHp = 1000;
    private SpriteRenderer _renderer;

    void Awake(){
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable() {
        PlayPopSFX();
    }

    void Update()
    {
        if(!_isHold)
            transform.Translate(Vector3.down * _speed * Time.deltaTime);
        if(_hp <= 0 || transform.position.y <= BoardManager.Instance.Border.yMin) {
            BoardManager.Instance.DestroyFeed(this);
            return;
        }
        Color c = _renderer.color;
        c.a = (float)_hp / _maxHp;
        _renderer.color = c;
    }

    public void Init(){
        _hp = _maxHp;
        _isHold = false;
    }

    public void Hold(){
        _isHold = true;
    }

    public void Eat(float power){
        _hp -= power;
    }

    public void PlayPopSFX(){
        _popSFX.pitch = Random.Range(0.7f, 1.3f);
        _popSFX.Play();
    }
}
