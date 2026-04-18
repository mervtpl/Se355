using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    
    public GameObject player;
    
    //example event. GUIManager can subscribe to this for updating hp of the player.
    public event Action<double> OnHealthChange;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //if scene's will change over time.
        //DontDestroyOnLoad(gameObject);
    }

    public void TakeDamage(double damage)
    {
        //update player's health.
        OnHealthChange?.Invoke(damage);
    }
}
