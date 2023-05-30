using System.Collections;
using System.Collections.Generic;
using Managers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class JuggernautLifeGrid : MonoBehaviour
{
    
    private HorizontalLayoutGroup _lifeGrid;

    private List<Image> _lives = new List<Image>();

    private int _nextActiveHeart = 0;
    
    private readonly Color _visible = Color.white;

    private readonly Color _invisible = new Color(1, 1, 1, 0);
    
    // Start is called before the first frame update
    void Start()
    {
        _lifeGrid = GetComponent<HorizontalLayoutGroup>();
    }


    public void SetLifeGrid(int lives, GameObject lifeObj)
    {
        for (int i = 0; i < lives; ++i)
        {
            var life = Instantiate(lifeObj, this.transform, true);
            var img = life.GetComponent<Image>();
            img.color = _invisible;
            _lives.Add(img);
        }
        _nextActiveHeart = lives - 1;
    }

    public void EliminateLife()
    {
        if (!(_nextActiveHeart < 0))
        {
            _lives[_nextActiveHeart].color = _invisible;
            _nextActiveHeart -= 1;
        }
    }

    public void EnableLifeGrid()
    {
        for (int i = 0; i < _lives.Count; i++)
        {
            _lives[i].color = _visible;
        }

        _nextActiveHeart = _lives.Count - 1;
    }

    public void DisableAllLifeGrid()
    {
        for (int i = 0; i < _lives.Count; i++)
        {
            _lives[i].color = _invisible;
        }
        
        _nextActiveHeart = -1;
    }
}
