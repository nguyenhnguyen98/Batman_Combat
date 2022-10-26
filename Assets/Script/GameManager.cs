using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    static GameManager instance;

    [SerializeField]
    private Image _healbarSprite;

    public static GameManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        CheckGameManagerIsInScene();
    }

    public void UpdateHealthBar(float health)
    {
        _healbarSprite.fillAmount = health / 100;
    }

    void CheckGameManagerIsInScene()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        } else
        {
            Destroy(this.gameObject);
        }
    }
}
