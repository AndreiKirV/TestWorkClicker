using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private ParticleSystem _fire;
    
    private Vector3 _scaleDie = new Vector3(-0.5f,0.99f,-0.5f);
    private float _jumpForce = 1f;
    private float _destroyDelay = 3f;
    private float _speedStep = 0.2f;
    private Vector2 _startPointWorld; 
    private Vector2 _maxPointWorld;
    private Vector3 _target;
    private Vector3 _startPosition;
    private float _speed;
    private float _startSpeed;
    private Rigidbody _rigidbody;
    private int _pressesToDeath;
    private bool _isDead = false;
    private Item _booster;

    public int PressesToDeath => _pressesToDeath;

    private void Awake() 
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start() 
    {
        _startPosition = transform.position;
        SetTarget();
        _startSpeed = _speed;
    }

    private void Update() 
    {
        TryMove();
    }

    private void SetTarget()
    {
        _target = new Vector3(Random.Range(_startPointWorld.x, _maxPointWorld.x), transform.position.y, Random.Range(_startPointWorld.y, _maxPointWorld.y));
        transform.LookAt(_target);
    }

    private void TryMove ()
    {  
        if (_isDead != true)
        {
            if (_startPosition.x != transform.position.x && _startPosition.y != transform.position.y)
                {
                    _startPosition = transform.position;
                }
            else
                {
                    SetTarget();
                    _speed += _speedStep;
                }
            
            transform.position = Vector3.MoveTowards(transform.position, _target, _speed * Time.deltaTime);
        }      
    }

    private void OnCollisionEnter(Collision other) 
    {
        if ((other.gameObject.TryGetComponent<Enemy>(out Enemy enemy)|| other.gameObject.TryGetComponent<Wall>(out Wall wall)) && _isDead == false)
            SetTarget();
    }

    private void OnCollisionStay(Collision other) 
    {
        if (other.gameObject.TryGetComponent<WorldController>(out WorldController worldController) && _isDead == false)
            _rigidbody.AddForce(0, _jumpForce, 0, ForceMode.VelocityChange);
    }

    private void Unfreeze()
    {
        _speed = _startSpeed;
    }

    public void Freeze (float freezingTime)
    {
        _speed = 0;
        Invoke("Unfreeze", freezingTime);
    }

    public void LightUp()
    {
        if (_fire != null)
            _fire.gameObject.SetActive(true);
    }

    public void SetPossibilityOfMovement (Vector2 startPoint, Vector2 maxPoint) 
    {
        _startPointWorld = new Vector2(startPoint.x, startPoint.y);
        _maxPointWorld = new Vector2(maxPoint.x, maxPoint.y);
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    public void SetPressToDeath(int pressesToDeath)
    {
        _pressesToDeath = pressesToDeath;
    }

    public void Die ()
    {
        _isDead = true;
        transform.localScale -= _scaleDie;
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        _rigidbody.isKinematic = true;

        if (TryGetComponent<AudioSource>(out AudioSource audio))
            audio.Play();
        

        if (TryGetComponent<CapsuleCollider>(out CapsuleCollider collider))
            Destroy(collider);
        
        if (_booster != null)
            Instantiate(_booster, transform.position, Quaternion.identity);

        if (TryGetComponent<Animator>(out Animator animator))
            animator.SetBool("Die", true);

        Destroy(gameObject, _destroyDelay);
    }

    public void AddBooster (Item booster) 
    {
        _booster = booster;
    }
}   