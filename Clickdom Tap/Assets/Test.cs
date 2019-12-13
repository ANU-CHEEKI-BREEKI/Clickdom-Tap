using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [ContextMenu("Start")]
    private void Start()
    {
        string debug = "";

        debug +=
            999f.ToShortFormattedString() + "\r\n" +
            1_999f.ToShortFormattedString() + "\r\n" +
            11_999f.ToShortFormattedString() + "\r\n" +
            111_999f.ToShortFormattedString() + "\r\n" +
            1_999_999f.ToShortFormattedString() + "\r\n" +
            11_999_999f.ToShortFormattedString() + "\r\n" +
            111_999_999f.ToShortFormattedString() + "\r\n" +
            1_999_999_999f.ToShortFormattedString() + "\r\n" +
            11_999_999_999f.ToShortFormattedString() + "\r\n" +
            111_999_999_999f.ToShortFormattedString();


        print(debug);
    }

}
