using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    public VisualElement rootElement;
    [SerializeField] private ParticleSystem goodParticle;
    [SerializeField] private ParticleSystem okParticle;
    [SerializeField] private ParticleSystem mehParticle;

    [SerializeField] private UIScript scriptManager;
    private VisualElement tutoPanel;    

    public Action OnInitButtons;

    public static UIManager instance;
    private void Awake()
    {
        instance = this;
        rootElement = uiDocument.rootVisualElement;
        tutoPanel = rootElement.Q<VisualElement>("tuto");
        InputSystem.actions.FindAction("Click").performed += CloseTuto;
    }


    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        OnInitButtons?.Invoke();
    }

    public void PlayGood()
    {
        goodParticle.Play();
    }

    public void PlayOk()
    {
        okParticle.Play();
    }

    public void PlayMeh()
    {
        mehParticle.Play();
    }

    private void CloseTuto(InputAction.CallbackContext context)
    {
        tutoPanel.style.display = DisplayStyle.None;
        InputSystem.actions.FindAction("Click").performed -= CloseTuto;
    }


}
