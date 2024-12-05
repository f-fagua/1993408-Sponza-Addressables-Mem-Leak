using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ButtonSet : MonoBehaviour
{
    public string Key;
    public Vector3 Position;
    
    public Button Load;
    public Button Release;
    public Button Destroy;
    public Button DestroyAndRelease;

    private void Start()
    {
        var director = FindObjectOfType<Director>();
        Initialise(Load, Key, x => director.Load(x, Position));
        Initialise(Release, Key, director.Release);
        Initialise(Destroy, Key, director.DestroyWithKey);
        Initialise(DestroyAndRelease, Key, director.DestroyAndReleaseWithKey);
    }

    void Initialise(Button button, string key, Action<string> func)
    {
        if (button)
        {
            var text = button.GetComponentInChildren<TMP_Text>();
            text.text += $" {key}";
            button.onClick.AddListener(() => func(key));
        }
    }
}