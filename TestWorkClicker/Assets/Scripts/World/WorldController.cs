using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[RequireComponent(typeof(RecordsManager))]
public class WorldController : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private List<Enemy> _enemies;
    [SerializeField] private List<Item> _items;
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _wall;

    [Header("Map configuration")]
    [SerializeField] private Vector2Int _size;
    [SerializeField] private float _offSet;

    [Header("Enemy configuration")]
    [SerializeField] private int _maxEnemies;
    [SerializeField] private int _pressesToDeath;
    [SerializeField] private int _enemiesToIncreasLives;
    [SerializeField] private int _enemiesToIncreasSpeed;
    [SerializeField] private int _chanceOfBooster;
    [SerializeField] private float _delaySpawn; 
    [SerializeField] private float _minDelay;
    [SerializeField] private float _timeStep;
    [SerializeField] private float _speedEnemies;
    [SerializeField] private float _freezeTimeOfSpawn;
    [SerializeField] private float _freezingTime;

    [Header("Panels")]
    [SerializeField] private TextMeshProUGUI _counterEnemies;
    [SerializeField] private TextMeshProUGUI _counterScore;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private GameObject _titersPanel;
    [SerializeField] private GameObject _recordsPanel;

    [Header("Audio")]

    [SerializeField] private AudioSource _touch;
    [SerializeField] private AudioSource _gameOver;
    [SerializeField] private AudioSource _titleMusic;

    private UnityAction EnemyLimitReached;
    private UnityAction FreezingSpawnReached;
    private UnityAction BombReached;
    private UnityAction FreezingAllReached;

    private Vector2Int [,] _gridWorld;
    private List<Enemy> _existingEnemies = new List<Enemy>();
    private float _startTime;
    private int _totalEnemiesCreated;
    private int _score = 0;
    private bool _isSpawned = true;
    private bool _isFirstTime = true;
    private Pointer _pointer;
    
    public Vector2Int Size => _size;
    public float OffSet => _offSet;
    public int Score => _score;

    private void Awake() 
    {
        _gridWorld = new Vector2Int [_size.x, _size.y];
        _camera.GetComponent<CameraController>().SetMaximumPosition(_size);
        _startTime = _delaySpawn;

        if (_camera.TryGetComponent<Pointer>(out Pointer pointer))
            _pointer = pointer;
    }

    private void Start() 
    {
        BuildWalls();
        EnemyLimitReached += GameOver;
        FreezingSpawnReached += FreezingSpawn;
        BombReached += SetFireEverything;
        BombReached += KillAll;
        FreezingAllReached += FreezeAll;
        _titleMusic.Play();
    }

    private void Update() 
    { 
        Spawn();
        TouchRay();
    }

    private void BuildWalls()
    {
        GameObject tempWall = Instantiate(_wall, new Vector3( -_offSet, _offSet, _size.y/2), Quaternion.identity);
        tempWall.transform.localScale = new Vector3 (tempWall.transform.localScale.x,tempWall.transform.localScale.y,_size.y+tempWall.transform.localScale.z*2);

        tempWall = Instantiate(_wall, new Vector3(_size.x + _offSet, _offSet, _size.y/2), Quaternion.identity);
        tempWall.transform.localScale = new Vector3 (tempWall.transform.localScale.x,tempWall.transform.localScale.y,_size.y+tempWall.transform.localScale.z*2);

        tempWall = Instantiate(_wall, new Vector3(_size.x/2 + _offSet, _offSet,  -_offSet), Quaternion.identity);
        tempWall.transform.localScale = new Vector3 (_size.x,tempWall.transform.localScale.y,tempWall.transform.localScale.z);

        tempWall = Instantiate(_wall, new Vector3(_size.x/2 + _offSet, _offSet, _size.y + _offSet), Quaternion.identity);
        tempWall.transform.localScale = new Vector3 (_size.x,tempWall.transform.localScale.y,tempWall.transform.localScale.z);
    }

    private void Spawn()
    {
        Enemy tempEnemy;

        if (_existingEnemies.Count <= 0 && _isFirstTime)
            {
                CreateEnemy(out tempEnemy);
                _isFirstTime = false;
            }

        if (_startTime > 0)
            _startTime -= Time.deltaTime;
        else if (_startTime <= 0 && _isSpawned)
        {
            CreateEnemy(out tempEnemy);
            _startTime = Random.Range(_minDelay, _delaySpawn);
            
            if (_delaySpawn >= _minDelay && (_delaySpawn - _timeStep) >= _minDelay)
                _delaySpawn -= _timeStep;
            else if((_delaySpawn - _timeStep) <= _minDelay)
                _delaySpawn = _minDelay;

            if (_existingEnemies.Count >= _maxEnemies)
                _isSpawned = false;
        }
    }

    private void KillAll()
    {
        _score += _existingEnemies.Count;
        UpdateText(ref _counterScore, _score);

        for (int i = 0; i < _existingEnemies.Count; i++)
        {
            _existingEnemies[i].Die();
            _pointer.DeleteTarget(_existingEnemies[i]);
        }

        _existingEnemies.Clear();
    }

    private void FreezeAll()
    {
        for (int i = 0; i < _existingEnemies.Count; i++)
        {
            _existingEnemies[i].Freeze(_freezingTime);
        }
    }

    private void SetFireEverything()
    {
        for (int i = 0; i < _existingEnemies.Count; i++)
        {
            _existingEnemies[i].LightUp();
        }
    }

    private bool ImplementRandomDrop ()
    {
        if (Random.Range(0, 100) <= _chanceOfBooster)
            return true;
        else
            return false;
    }

    private void FreezingSpawn()
    {
        _isSpawned = false;
        Invoke("RepealFreezingSpawn", _freezeTimeOfSpawn);
    }

    private void RepealFreezingSpawn()
    {
        _isSpawned = true;
    }

    private void CreateEnemy(out Enemy tempEnemy)
    {
        tempEnemy = Instantiate(_enemies[Random.Range(0,_enemies.Count)], new Vector3(Random.Range(0,_size.x)+_offSet, 0, Random.Range(0,_size.y) + _offSet),  Quaternion.identity);
        tempEnemy.SetPossibilityOfMovement(new Vector2(0 + _offSet, 0 + _offSet), new Vector2(_size.x + _offSet, _size.y + _offSet));
        tempEnemy.SetSpeed(_speedEnemies);
        tempEnemy.SetPressToDeath(_pressesToDeath);
        _pointer.AddTarget(tempEnemy);

        if (ImplementRandomDrop())
            tempEnemy.AddBooster(_items[Random.Range(0, _items.Count)]);
        
        _existingEnemies.Add(tempEnemy);
        _totalEnemiesCreated++;

        if (_totalEnemiesCreated%_enemiesToIncreasLives == 0)
            _pressesToDeath++;

        if (_totalEnemiesCreated%_enemiesToIncreasSpeed == 0)
            _speedEnemies += 0.5f;

        if(_existingEnemies.Count >= _maxEnemies)
            {
                EnemyLimitReached?.Invoke();
            }

        UpdateText(ref _counterEnemies, _existingEnemies.Count);
    }

    private void TouchRay()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                _touch.Play();

                if (hit.collider.gameObject.TryGetComponent<Enemy>(out Enemy enemy))
                {
                    enemy.SetPressToDeath(enemy.PressesToDeath-1);

                    if (enemy.PressesToDeath == 0)
                    {
                        _existingEnemies.Remove(enemy);
                        _pointer.DeleteTarget(enemy);
                        enemy.Die();
                        UpdateText(ref _counterEnemies, _existingEnemies.Count);
                        _score++;
                        UpdateText(ref _counterScore, _score);
                    }                    
                }
                else if(hit.collider.gameObject.TryGetComponent<Potion>(out Potion potion))
                {
                    if(potion.TryGetComponent<IceCube>(out IceCube iceCube))
                    FreezingAllReached?.Invoke();

                    potion.Crush();
                    potion.Crak();
                    FreezingSpawnReached?.Invoke();
                }
                else if(hit.collider.gameObject.TryGetComponent<Bomb>(out Bomb bomb))
                {
                    bomb.Crush();
                    BombReached?.Invoke();
                }
            }
        }
    }

    private void UpdateText(ref TextMeshProUGUI targetText, string text)
    {
        targetText.text = text;
    }

    private void UpdateText(ref TextMeshProUGUI targetText, int targetValue)
    {
        targetText.text = targetValue.ToString();
    }

    private void ChangeStateOfObject(GameObject panel, bool condition)
    {
        panel.SetActive(condition);
    }

    public void GameOver()
    {
        _gameOver.Play();
        GetComponent<RecordsManager>().SetScore(_score);

        ChangeStateOfObject(_gameOverPanel, true);
        Time.timeScale = 0;
    }

    public void RestartGame () 
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Exit ()
    {
        Application.Quit();
    }

    public void ActivateMenu()
    {
        Time.timeScale = 0;

        if(_menuPanel.activeInHierarchy == false)
            ChangeStateOfObject(_menuPanel, true);

       if(_gameOverPanel.activeInHierarchy)
            ChangeStateOfObject(_gameOverPanel, false);

        if(_titersPanel.activeInHierarchy)
            ChangeStateOfObject(_titersPanel, false);

        if(_recordsPanel.activeInHierarchy)
            ChangeStateOfObject(_recordsPanel, false);
    }

    public void ActivateTiters()
    {
        ChangeStateOfObject(_menuPanel, false);
        ChangeStateOfObject(_titersPanel, true);
    }

    public void ActivateRecords()
    {
        ChangeStateOfObject(_recordsPanel, true);
        GetComponent<RecordsManager>().PrintScore();
    }
}