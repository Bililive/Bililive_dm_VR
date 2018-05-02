using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class TimeDisplayer : MonoBehaviour
{

    private TextMesh mesh;
    private Coroutine coroutine;

    private void Awake()
    {
        mesh = GetComponent<TextMesh>();
    }

    private void OnEnable()
    {
        coroutine = StartCoroutine(Refresh());
    }

    private void OnDisable()
    {
        StopCoroutine(coroutine);
    }

    IEnumerator Refresh()
    {
        while (true)
        {
            mesh.text = "当前时间: " + DateTime.Now.ToLongTimeString();
            yield return new WaitForSecondsRealtime(1);
        }
    }
}
