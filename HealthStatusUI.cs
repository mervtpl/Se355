using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Slider health_slider;
    public PlayerCharacter player;

    void Start()
    {
        if (player != null)
        {
            health_slider.maxValue = player.max_hp;
            health_slider.value = player.max_hp;
        }
    }

    void Update()
    {
        health_slider.value = Mathf.Lerp(health_slider.value, player.GetHP(), Time.deltaTime * 10f);
    }
}