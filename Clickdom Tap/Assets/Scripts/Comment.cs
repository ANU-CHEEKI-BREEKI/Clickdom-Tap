using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Comment : MonoBehaviour
{
    [SerializeField] [Multiline(7)] 
    private string comment = "Это комментарий для разработчика в UnityEditor";
}
