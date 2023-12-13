using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject>    _levels;

    private int                 _currentLevel   = 0;

    public void NextLevel()
    {
        _levels[_currentLevel + 1].SetActive(true);
        _levels[_currentLevel + 1].GetComponent<LevelController>().PlacePlayer();
        _levels[_currentLevel].SetActive(false);
        _currentLevel++;
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (var level in _levels)
            level.SetActive(false);
        _levels[_currentLevel].SetActive(true);
        _levels[_currentLevel].GetComponent<LevelController>().PlacePlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
