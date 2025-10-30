using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    public VisualElement rootElement;

    [SerializeField] private UIScript scriptManager;

    public Action OnInitButtons;

    public static UIManager instance;
    private void Awake()
    {
        instance = this;
        rootElement = uiDocument.rootVisualElement;
    }

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        OnInitButtons?.Invoke();
    }

    

}
