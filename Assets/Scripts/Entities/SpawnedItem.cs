using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnedItem : MonoBehaviour
{
   private bool hasCollided = false;

   public bool HasCollided 
   {
       get 
       {
           return hasCollided;
       }
       set 
       {
           hasCollided = true;
       }
   }
}
