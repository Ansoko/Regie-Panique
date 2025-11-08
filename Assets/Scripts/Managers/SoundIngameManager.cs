using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static DisksManager;

public class SoundIngameManager : MonoBehaviour
{
    [SerializeField] private List<string> nameSounds = new();
    [SerializeField] private List<AudioClip> listSounds = new();
    private int currentIndex = 0;

    private Button nextSound, previousSound;
    private Label labelSound;

    private void Start()
    {
        UIManager.instance.OnInitButtons += InitButtons;
        DisksManager.instance.OnPlayDisk += PlayDisc;
        DisksManager.instance.OnCreateDisk += RetreaveParameters;

        nameSounds.Insert(0, "Pas de bruitage");
        listSounds.Insert(0, null);
    }
    private void InitButtons()
    {
        VisualElement root = UIManager.instance.rootElement.Q("sound");
        nextSound = root.Q<Button>("nextSound");
        nextSound.clicked += NextSound;
        previousSound = root.Q<Button>("previousSound");
        previousSound.clicked += PreviousSound;
        labelSound = root.Q<Label>("nameSound");

        labelSound.text = nameSounds[currentIndex];
    }

    private void ResetButtons()
    {
        currentIndex = 0;
        labelSound.text = nameSounds[currentIndex];
    }

    private void NextSound(){
        currentIndex++;
        if (currentIndex >= nameSounds.Count)
            currentIndex = 0;
        labelSound.text = nameSounds[currentIndex];
    }

    private void PreviousSound()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = nameSounds.Count - 1;
        labelSound.text = nameSounds[currentIndex];
    }
    private void RetreaveParameters(Disk currentDisc)
    {
        SnapshotDisc.DiscData data = SnapshotDisc.DiscData.CreateDefault();
        data.soundType = nameSounds[currentIndex];

        ResetButtons();

        currentDisc.AddDataToDisk(data);
    }

    private void PlayDisc(Disk currentDisc)
    {
        if(currentDisc.data.soundType == "Pas de bruitage") return;
        AudioManager.Instance.PlaySFX(listSounds[nameSounds.IndexOf(currentDisc.data.soundType)]);
    }
}
