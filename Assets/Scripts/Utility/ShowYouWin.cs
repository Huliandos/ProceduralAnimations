using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowYouWin : MonoBehaviour
{
    [SerializeField]
    Canvas canvas;

    private void OnTriggerEnter(Collider other)
    {
        canvas.enabled = true;
    }
}
