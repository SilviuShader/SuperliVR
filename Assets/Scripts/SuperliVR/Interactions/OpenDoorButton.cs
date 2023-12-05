using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoorButton : MonoBehaviour
{
    [SerializeField]
    private GameObject  _door;
    [SerializeField]
    private Material    _transparentMaterial;
    [SerializeField]
    private Material    _opaqueMaterial;
    private int         _objectsOnButton = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 6 || collision.gameObject.layer == 7)
        {
            _door.GetComponent<MeshRenderer>().material = _transparentMaterial;
            _door.GetComponent<BoxCollider>().enabled = false;
            _objectsOnButton++;
        }
            
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 6 || collision.gameObject.layer == 7)
        {
            _objectsOnButton--;
            if (_objectsOnButton == 0)
            {
                _door.GetComponent<MeshRenderer>().material = _opaqueMaterial;
                _door.GetComponent<BoxCollider>().enabled = true;
            }
        }
            
    }
}
