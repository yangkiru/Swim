using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource BGMusic;
    
    private float _t;

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        if (!BGMusic.isPlaying) {
            BGMusic.Play();
            BGMusic.volume = 0;
            _t = 0;
        }
        if (BGMusic.isPlaying && BGMusic.time > BGMusic.clip.length * 0.9f){
            BGMusic.volume -= 0.0001f;
            if (BGMusic.volume <= 0) {
                BGMusic.time = 0;
                BGMusic.Stop();
            }
        } else if (_t < 1) {
            _t += Time.deltaTime * 0.001f;
            BGMusic.volume = Mathf.Lerp(BGMusic.volume, 1, _t);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
            BGMusic.time += 5;
    }
}
