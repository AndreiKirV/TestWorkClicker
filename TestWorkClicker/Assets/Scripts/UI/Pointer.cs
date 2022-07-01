using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;

public class Pointer : MonoBehaviour
{
    [SerializeField] private GameObject _icon;
    [SerializeField] private Canvas _canvas;

    private Camera _camera;
    private Dictionary<Enemy, GameObject> _icons = new Dictionary<Enemy, GameObject>();

    private void Awake() 
    {
        if (TryGetComponent<Camera>(out Camera camera))
        _camera = camera;
    }

    private void Update() 
    {
        SetPositionForIcons();
    }

    private GameObject CreateIcon()
    {
        GameObject _tempIcon = GameObject.Instantiate<GameObject>(_icon);
        _tempIcon.gameObject.transform.SetParent(_canvas.transform);
        return _tempIcon;
    }

    private void SetPositionForIcons()
    {
        float minX;
        float maxX;
        float minY;
        float maxY;
        Vector2 tempPosition;

        for (int i = 0; i < _icons.Count; i++)
            {

                Image tempImage = _icons.ElementAt(i).Value.GetComponent<Image>();
                minX = tempImage.GetPixelAdjustedRect().width / 2;
                maxX = Screen.width - minX;

                minY = tempImage.GetPixelAdjustedRect().height / 2;
                maxY = Screen.height - minY;

                tempPosition = _camera.WorldToScreenPoint(_icons.ElementAt(i).Key.transform.position);

                if (tempPosition.x > maxX || tempPosition.x < minX || tempPosition.y > maxY || tempPosition.y < minY)
                {
                    tempImage.gameObject.SetActive(true);
                    tempPosition.x = Mathf.Clamp(tempPosition.x, minX, maxX);
                    tempPosition.y = Mathf.Clamp(tempPosition.y, minY, maxY);
                    _icons.ElementAt(i).Value.transform.position = tempPosition;
                }
                else
                tempImage.gameObject.SetActive(false);
            }
    }

    public void DeleteTarget(Enemy target)
    {
        GameObject tempIcon = _icons[target];
        _icons.Remove(target);
        Destroy(tempIcon);
    }

    public void AddTarget(Enemy target)
    {
        _icons.Add(target, CreateIcon());
    }
}