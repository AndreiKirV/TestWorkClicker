using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private float _positionY;
    [SerializeField] private float _maxPositionY;
    [SerializeField] private float _minPositionY;
    [SerializeField] private Vector2 _offset;
    [SerializeField] private float _speed;

    private Camera _camera;
    private Vector3 _touch;
    private Vector2Int _maxSize;

    private void Awake() 
    {
        _camera = GetComponent<Camera>();
    }

    private void Start() 
    {
        SetStartposition();
    }

    private void Update() 
    {
        Move();
    }

    public void SetMaximumPosition(Vector2Int maxSize)
    {
        _maxSize = maxSize;
    }

    private void SetStartposition()
    {
        transform.position = new Vector3(_maxSize.x/2 + _offset.x, _positionY, _maxSize.y/2 + _offset.y);
    }

    private void Move()
    {
        if (Input.GetMouseButtonDown(0))
                _touch = _camera.ScreenToViewportPoint(Input.mousePosition);

        if (Input.GetMouseButton(0))
            {
                Vector3 tempDirection = new Vector3(_touch.x - _camera.ScreenToViewportPoint(Input.mousePosition).x, _touch.y - _camera.ScreenToViewportPoint(Input.mousePosition).y, _touch.z)* Time.deltaTime * _speed;
                Vector3 targetPosition = new Vector3(transform.position.x + tempDirection.x, transform.position.y, transform.position.z + tempDirection.y);

                if (targetPosition.x >= 0 && targetPosition.x <= _maxSize.x && targetPosition.z >= 0 && targetPosition.z <= _maxSize.y)
                    transform.position = targetPosition;
                
            }
        
        TryChangeYPosition();
    }

    private void TryChangeYPosition()
    {
        if (Input.touchCount == 2)
        {
            Touch touchOne = Input.GetTouch(0);
            Touch touchTwo = Input.GetTouch(1);

            Vector2 touchOneStartPosition = touchOne.position - touchOne.deltaPosition;
            Vector2 touchTwoStartPosition = touchTwo.position - touchTwo.deltaPosition;

            float startDistanceTouch = (touchOneStartPosition - touchTwoStartPosition).magnitude;
            float currentDistanceTouch = (touchOne.position - touchTwo.position).magnitude;
            float difference = currentDistanceTouch - startDistanceTouch;

            RepositionByY(difference * 0.01f);
        }
    }

    private void RepositionByY(float increment)
    {
        transform.position =  new Vector3 (transform.position.x, Mathf.Clamp(transform.position.y - increment, _minPositionY, _maxPositionY), transform.position.z);
    }
}