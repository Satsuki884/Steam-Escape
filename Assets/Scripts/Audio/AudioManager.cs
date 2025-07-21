using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Audio Clips Music")]
    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private AudioClip _gameMusic;

    [Header("Audio Clips SFX")]
    [SerializeField] private AudioClip _bombMusic;
    [SerializeField] private AudioClip _bonusMusic;
    [SerializeField] private AudioClip _enemyHitMusic;
    [SerializeField] private AudioClip _explosionSoundMusic;
    [SerializeField] private AudioClip _explosionHitMusic;
    [SerializeField] private AudioClip _endGameMusic;
    [SerializeField] private AudioClip _newLevelMusic;


    private string _menu = "Menu";
    // private string _game = "Game";

    // [SerializeField] private string Bomb = "bomb";
    // [SerializeField] private string EnemyHit = "enemyHit";
    // [SerializeField] private string Bonus = "bonus";
    // [SerializeField] private string ExplosionSound = "explosionSound";
    // [SerializeField] private string ExplosionHit = "explosionHit";
    // [SerializeField] private string EndGame = "endGame";
    // [SerializeField] private string NewLevel = "newLevel";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        SetMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetMusicForScene(scene.name);
    }

    private void SetMusicForScene(string sceneName)
    {
        if (_musicSource == null) return;

        AudioClip targetMusic = sceneName == _menu ? _menuMusic : _gameMusic;

        if (_musicSource.clip != targetMusic)
        {
            _musicSource.clip = targetMusic;
            _musicSource.Play();
        }
    }

    public void PlaySFX(string clipName)
    {
        switch (clipName)
        {
            case "bomb":
                _sfxSource.PlayOneShot(_bombMusic);
                break;
            case "enemyHit":
                _sfxSource.PlayOneShot(_enemyHitMusic);
                break;
            case "bonus":
                _sfxSource.PlayOneShot(_bonusMusic);
                break;
            case "explosionSound":
                _sfxSource.PlayOneShot(_explosionSoundMusic);
                break;
            case "explosionHit":
                _sfxSource.PlayOneShot(_explosionHitMusic);
                break;
            case "endGame":
                _sfxSource.PlayOneShot(_endGameMusic);
                break;
            case "newLevel":
                _sfxSource.PlayOneShot(_newLevelMusic);
                break;  
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
