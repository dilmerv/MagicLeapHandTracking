using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    [SerializeField]
    private float points = 0;

    [SerializeField]
    private float incrementedBy = 1;

    [SerializeField]
    private TextMeshPro scoreText = null;

    // Global Score
    public static float TotalPoints = 0;

    void OnTriggerExit(Collider other) 
    {
        if(scoreText != null)
        {
            SpawnedItem spawnedItem = other.GetComponent<SpawnedItem>();
            if(spawnedItem != null && !spawnedItem.HasCollided){
                spawnedItem.HasCollided = true;

                // increment local points
                points += incrementedBy;
                
                // increment global points
                TotalPoints += points;
                
                scoreText.text = $"{points}";
            }
        }
    }
}
