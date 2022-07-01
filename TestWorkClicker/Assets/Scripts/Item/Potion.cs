using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : Item
{
    [SerializeField] private GameObject _bootle;

    [SerializeField] private List<GameObject> _shards;

    private float _timeDestroyCell = 2;

    public void Crak ()
    {
        Destroy(_bootle);

        for (int i = 0; i < _shards.Count; i++)
        {
            GameObject tempShard = _shards[i];
            if(tempShard.TryGetComponent<Rigidbody>(out Rigidbody rigidbody) && tempShard.TryGetComponent<MeshCollider>(out MeshCollider collider))
                {
                    rigidbody.isKinematic = false;
                    tempShard.GetComponent<MeshCollider>().enabled = true;
                }

            tempShard.transform.parent = null;
            Destroy(tempShard, _timeDestroyCell);
        }
    }
}