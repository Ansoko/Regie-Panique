using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static DisksManager;

public class TimelineManager : MonoBehaviour
{
    public float speed = 1f;
    public float timeBetweenScene = 30f;
    [SerializeField] private List<float> durationTimestamp;
    [SerializeField] private VisualTreeAsset timelineTemplate;
    private ScrollView timelineHolder;
    private float totalDuration = 0;
    private float widthTimeline = 0;
    private VisualElement currentplayingElement;
    private int currentPlayingKey = 0;
    private float elipsedTime = 0f;
    private float widthPerSecond = 200f;

    private Dictionary<VisualElement, Disk> timelineCases = new();
    private bool isStarted = false;

    public static TimelineManager instance;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //TEMP todo: associer pour chaque timestamp un temps
        durationTimestamp.Clear();
        foreach (TextContent.Dialogue dialogue in TextContent.instance.timestamps)
        {
            if (dialogue.key < 1) continue;

            if (dialogue.character == "Rideau")
            {
                durationTimestamp.Add(timeBetweenScene);
                totalDuration += timeBetweenScene;
            }
            else
            {
                durationTimestamp.Add(1);
                totalDuration += 1;
            }
        }

        UIManager.instance.OnInitButtons += InitUI;
        DisksManager.instance.OnPlayDisk += PlayDisc;
    }

    private void InitUI()
    {
        VisualElement root = UIManager.instance.rootElement.Q("timeline");
        timelineHolder = root.Q<ScrollView>("timelineScroll");
        timelineHolder.contentContainer.style.width = widthPerSecond * totalDuration;
        widthTimeline = widthPerSecond * totalDuration;
        for (int i = 0; i < durationTimestamp.Count; i++)
        {
            VisualElement timelineInstance = timelineTemplate.Instantiate();
            VisualElement rootElement = timelineInstance.Q("case");
            Label tiradeTimelineLabel = timelineInstance.Q<Label>("tirade-timeline");
            Label timecodeLabel = timelineInstance.Q<Label>("timecode");

            rootElement.style.width = durationTimestamp[i] * widthPerSecond;
            timelineInstance.RegisterCallback<PointerDownEvent>(OnPointerDown);

            TextContent.Dialogue dialogue = TextContent.instance.FindKeyframe(i+1);
            tiradeTimelineLabel.text = dialogue.dialogue=="null" ? 
                dialogue.character == "Rideau" ? "Fin d'acte" : "Début d'acte" 
                : $"{dialogue.character}<br>{dialogue.dialogue}";
            timecodeLabel.text = $"{dialogue.key}";

            timelineHolder.contentContainer.Add(timelineInstance);
            timelineCases.Add(timelineInstance, null);
        }
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        VisualElement timeCase = (VisualElement) evt.currentTarget;
        if(timeCase == null) return;

        if (timelineCases[timeCase] == null) return;
        
        DisksManager.instance.ShowDisk(timelineCases[timeCase]);
        timelineCases[timeCase] = null;
        return;
    }

    public bool PlaceDiskOnTimeline(Disk snapshotDisc, Vector2 pointerPosition)
    {
        VisualElement hoveringElement = IsOver(pointerPosition);
        if (hoveringElement == null) return false;

        if (timelineCases[hoveringElement] != null)
        {
            //DisksManager.instance.ShowDisk(timelineCases[timeCase], false); //on n'expulse plus l'ancienne disquette
            timelineCases[hoveringElement].AddDataToDisk(snapshotDisc.data);
        }
        else
        {
            timelineCases[hoveringElement] = snapshotDisc;
        }

        if (currentplayingElement == hoveringElement)
        {
            DisksManager.instance.PlayDisk(timelineCases[hoveringElement]);
        }

        if (!isStarted)
        {
            isStarted = true;
            StartCoroutine(PlayTimeline());
        }
        return true;
    }

    public VisualElement IsOver(Vector2 pointerPosition)
    {
        bool firstElementFound = false;
        foreach (var rect in timelineCases.Keys)
        {
            if (GetFirstVisibleElement() != rect && !firstElementFound)
                continue;

            firstElementFound = true;
            if (rect.worldBound.Contains(pointerPosition))
            {
                return rect;
            }
        }
        return null;
    }

    private int nbrDiscPlayed = 0;


    private IEnumerator PlayTimeline()
    {
        StartCoroutine(ActorManager.instance.StartThePlay());
        elipsedTime = 0f;
        while (elipsedTime < totalDuration)
        {
            timelineHolder.scrollOffset = new Vector2((elipsedTime / totalDuration) * widthTimeline, 0);

            VisualElement firstVisible = GetFirstVisibleElement();
            if(currentplayingElement != firstVisible) //début d'une nouvelle case
            {
                CheckScoring(currentPlayingKey, currentplayingElement);

                currentPlayingKey++;
                currentplayingElement = firstVisible;
                nbrDiscPlayed = 0;

                DisksManager.instance.PlayDisk(timelineCases[currentplayingElement]??null);
            }

            elipsedTime += Time.deltaTime * speed;
            yield return null;
        }
    }

    private void PlayDisc(Disk _)
    {
        nbrDiscPlayed++;
    }

    private VisualElement GetFirstVisibleElement()
    {
        float scrollX = (elipsedTime / totalDuration) * widthTimeline;
        float viewWidth = timelineHolder.contentViewport.resolvedStyle.width;

        foreach (var child in timelineHolder.contentContainer.Children())
        {
            float childLeft = child.layout.x;
            float childRight = child.layout.x + child.layout.width;

            if (childRight > scrollX && childLeft < scrollX + viewWidth)
            {
                return child;
            }
        }

        return null;
    }

    private void CheckScoring(int key, VisualElement timeElementToCheck)
    {
        if(key < 1) return;
        if(timelineCases.ContainsKey(timeElementToCheck) == false) return;

        Disk playingDisk = timelineCases[timeElementToCheck];

        TextContent.Dialogue dialogue = TextContent.instance.FindKeyframe(key);
        SnapshotDisc.DiscData scene = DisksManager.instance.GetSceneParameters();
        playingDisk ??= new Disk() { data = SnapshotDisc.DiscData.CreateDefault() };

        int nbrErrors = 0;

        Debug.Log($"Vérification du key {key}...");
        if(dialogue.rideau == "Fermé" && scene.curtainOpening == 0)
        {
            Debug.Log(" - rideau fermé, pas d'autres vérifications");
        }
        else if ((dialogue.rideau == "Fermé" && scene.curtainOpening == 1)
            || (dialogue.rideau == "Ouvert" && scene.curtainOpening == 0))
        {
            //rideau
            nbrErrors += 12; //all error !!!!!
            Debug.Log(" - rideau devrait être fermé mais scène ouverte ou inversement");
        }
        else
        {
            //spot
            if (dialogue.onOffSpot == "Allumé")
            {
                if (dialogue.onOffSpot != scene.hasSpot)
                {
                    nbrErrors += 5;
                    Debug.Log(" - spot devrait être allumé mais éteint");
                }
                else
                {
                    if (LightManager.instance.spotColorDict[dialogue.couleurSpot] != scene.spotColor)
                    {
                        nbrErrors++;
                        Debug.Log(" - mauvaise couleur de spot ("+ LightManager.instance.spotColorDict[dialogue.couleurSpot]+" attendu, mais current: "+ scene.spotColor+")");
                    }
                    if (dialogue.diametreSpot == "Petit" && scene.spotSize == 1
                        || dialogue.diametreSpot == "Grand" && scene.spotSize == 0)
                    {
                        nbrErrors++;
                        Debug.Log(" - mauvais diamètre de spot, attendu: "+ dialogue.diametreSpot +", joué:" +scene.spotSize);
                    }
                    if (dialogue.placementGrille9 != (scene.spotPlacement+1))
                    {
                        nbrErrors++;
                        Debug.Log(" - mauvais placement de spot, attendu: "+ dialogue.placementGrille9+ ", joué: "+ scene.spotPlacement);
                    }
                    if (dialogue.mouvement == "Fixe" && !(playingDisk.data.spotMouvement == -1)
                        || dialogue.mouvement == "G 2" && playingDisk.data.spotMouvement == 0
                        || dialogue.mouvement == "G 1" && playingDisk.data.spotMouvement == 1
                        || dialogue.mouvement == "D 1" && playingDisk.data.spotMouvement == 2
                        || dialogue.mouvement == "D 2" && playingDisk.data.spotMouvement == 3)
                    {
                        nbrErrors++;
                        Debug.Log(" - mauvais mouvement de spot");
                    }
                }
            }
            else if (dialogue.onOffSpot != scene.hasSpot)
            {
                nbrErrors++;
                Debug.Log(" - spot devrait être éteint mais allumé");
            }

            //ambiance
            if (dialogue.intensiteAmb > 0 && dialogue.intensiteAmb != scene.ambIntensity)
            {
                nbrErrors++;
                Debug.Log(" - mauvaise intensité ambiante, attendu: "+dialogue.intensiteAmb+", scène: "+scene.ambIntensity);
            }
            if (dialogue.couleurAmb != "null" && LightManager.instance.ambColorDict[dialogue.couleurAmb] != scene.ambColor)
            {
                nbrErrors++;
                Debug.Log(" - mauvaise couleur ambiante, attendu: "+ LightManager.instance.ambColorDict[dialogue.couleurAmb] + ", scène: "+scene.ambColor);
            }

            //decors
            if (dialogue.decorL1 != scene.decorsL1)
            {
                nbrErrors++;
                Debug.Log(" - mauvais décor L1, attendu: "+dialogue.decorL1+", posé: "+scene.decorsL1);
            }
            if (dialogue.decorL2 != scene.decorsL2)
            {
                nbrErrors++;
                Debug.Log(" - mauvais décor L2, attendu: "+dialogue.decorL2+", posé: "+scene.decorsL2);
            }
            if (dialogue.decorL3 != scene.decorsL3)
            {
                nbrErrors++;
                Debug.Log(" - mauvais décor L3, attendu: "+dialogue.decorL3+", posé: "+scene.decorsL3);
            }
        }

        //sound
        if (!dialogue.sons.Contains("Dev")
            && !(dialogue.sons == "null" && playingDisk.data.soundType == "Pas de bruitage")
            && dialogue.sons != playingDisk.data.soundType)
        {
            nbrErrors++;
            Debug.Log(" - mauvais son, attendu: "+dialogue.sons+", joué:"+playingDisk.data.soundType);
        }

        if(nbrDiscPlayed > 1)
        {
            nbrErrors += (nbrDiscPlayed - 1);
        }

        Debug.Log($"Key {key} - Disques joués: {nbrDiscPlayed} - Erreurs: {nbrErrors}");

        if(nbrErrors == 0)
        {
            AudioManager.Instance.PlaySoundByName("Great");
            UIManager.instance.PlayGood();
        }
        else if(nbrErrors <= 3)
        {
            AudioManager.Instance.PlaySoundByName("Good");
            UIManager.instance.PlayOk();
        }
        else
        {
            AudioManager.Instance.PlaySoundByName("Bad");
            UIManager.instance.PlayMeh();
        }
    }
}
