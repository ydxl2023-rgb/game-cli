using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Oathx.Bundle.Assets;

public class XHotfixBox : MonoBehaviour
{
    [SerializeField]
    private Button no;
    [SerializeField]
    private Button yes;
    [SerializeField]
    private Text context;

    public void MessageBox(string message, System.Action<bool, bool> click)
    {
        if (context)
            context.text = message;

        yes.onClick.AddListener(() => {
            click(true, false);
            gameObject.SetActive(false);
        });

        no.onClick.AddListener(() => {
            click(false, true);
            gameObject.SetActive(false);
        });

        gameObject.SetActive(true);
    }
}
