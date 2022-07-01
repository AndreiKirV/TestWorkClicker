using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class Item : MonoBehaviour
{
    [SerializeField] private ParticleSystem _effectDie;
    private Rigidbody _rigidbody;
    private int _forceValue = 5;

    private void Awake() 
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start() 
    {
        _rigidbody.AddForce(new Vector3(0, _forceValue, 0), ForceMode.Force);
    }

    public void Crush()
    {
        if (_effectDie != null)
            _effectDie.gameObject.SetActive(true);
        
        GetComponent<AudioSource>().Play();
        Destroy(gameObject, GetComponent<AudioSource>().clip.length);
    }
}