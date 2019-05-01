using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleClothController : MonoBehaviour
{
    private Cloth cloth;

    void Awake()
    {
        cloth = GetComponent<Cloth>();
    }
}
