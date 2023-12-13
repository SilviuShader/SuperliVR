using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class LevelController : MonoBehaviour
{
    [SerializeField]
    private GameObject _player;
    [SerializeField]
    private GameObject _startingPosition;

    public void PlacePlayer()
    {
        _player.transform.position = _startingPosition.transform.position;
    }
    // Start is called before the first frame update
    void Start()
    {
        PlacePlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
