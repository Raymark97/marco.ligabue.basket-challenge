using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [SerializeField]
    private TextMeshProUGUI playerScore, NPCScore;

    [SerializeField]
    private Slider fireSlider, shootSlider;


    public void SetShootingRanges(float directShotVel, float bankShotVel) {
        
        
    }

    public void SetShootValue(float shootValue) {
        shootSlider.value = shootValue;
    }

    public void SetPlayerPoints(int points) {
        playerScore.text = points.ToString();
    }

    public void SetNPCPoints(int points) {
        NPCScore.text = points.ToString();
    }
    
    
}
