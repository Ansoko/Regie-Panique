using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TimelineManager : MonoBehaviour
{
    public float speed = 1f;
    public float timeBetweenScene = 30f;
    [SerializeField] private List<float> durationTimestamp;
    [SerializeField] private VisualTreeAsset timelineTemplate;
    private ScrollView timelineHolder;
    private float totalDuration = 0;
    private float widthTimeline = 0;
    public VisualElement currentTarget;
    private VisualElement currentplayingElement;
    private int currentPlayingKey = 0;
    private float elipsedTime = 0f;
    private float widthPerSecond = 200f;

    private Dictionary<VisualElement, SnapshotDisc> timelineCases = new();
    private SnapshotDisc currentPlayingDisc = null;

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
        InputSystem.actions.FindAction("Click").started += ClickOverCase;
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
            rootElement.RegisterCallback<MouseEnterEvent>(HoverCase);
            rootElement.RegisterCallback<MouseLeaveEvent>(UnhoverCase);

            TextContent.Dialogue dialogue = TextContent.instance.FindKeyframe(i+1);
            tiradeTimelineLabel.text = dialogue.dialogue=="null" ? "Action" : $"{dialogue.character}<br>{dialogue.dialogue}";
            timecodeLabel.text = $"{dialogue.key}";

            timelineHolder.contentContainer.Add(timelineInstance);
            timelineCases.Add(timelineInstance, null);
        }

        StartCoroutine(PlayTimeline()); //temp
    }

    private void HoverCase(MouseEnterEvent evt)
    {
        currentTarget = evt.currentTarget as VisualElement;
        currentTarget.style.backgroundColor = Color.red;
    }

    private void UnhoverCase(MouseLeaveEvent evt)
    {
        VisualElement target = evt.currentTarget as VisualElement;
            target.style.backgroundColor = Color.white;
        if(currentTarget == target)
            currentTarget = null;
    }

    protected virtual void ClickOverCase(InputAction.CallbackContext ctx)
    {
        if (currentTarget == null) return;
        if (timelineCases[currentTarget.parent] == null) return;
        
        DisksManager.instance.ShowDisk(timelineCases[currentTarget.parent]);
        timelineCases[currentTarget.parent] = null;
        currentTarget.style.backgroundColor = Color.white;
        return;
        
    }

    public void PlaceDiskOnTimeline(SnapshotDisc snapshotDisc)
    {
        if (timelineCases[currentTarget.parent]!=null)
        {
            DisksManager.instance.ShowDisk(timelineCases[currentTarget.parent], false);
        }
        timelineCases[currentTarget.parent] = snapshotDisc;
        if (currentTarget != null)
            currentTarget.style.backgroundColor = Color.green;
    }

    private int nbrDiscPlayed = 0;
    private IEnumerator PlayTimeline()
    {
        elipsedTime = 0f;
        while (elipsedTime < totalDuration)
        {
            timelineHolder.scrollOffset = new Vector2((elipsedTime / totalDuration) * widthTimeline, 0);

            VisualElement firstVisible = GetFirstVisibleElement();
            if(currentplayingElement != firstVisible) //début d'une nouvelle case
            {
                CheckScoring(currentPlayingKey, nbrDiscPlayed);

                currentPlayingKey++;
                currentplayingElement = firstVisible;
                nbrDiscPlayed = 0;
            }

            if (firstVisible != null && currentPlayingDisc != timelineCases[firstVisible]) //changment de disk
            {
                currentPlayingDisc = timelineCases[firstVisible];
                if (currentPlayingDisc != null)
                {
                    nbrDiscPlayed++;
                    DisksManager.instance.PlayDisk(currentPlayingDisc);
                }
            }

            elipsedTime += Time.deltaTime * speed;
            yield return null;
        }
    }

    private VisualElement GetFirstVisibleElement()
    {
        float scrollX = (elipsedTime / totalDuration) * widthTimeline;
        float viewWidth = timelineHolder.contentViewport.resolvedStyle.width;

        foreach (var child in timelineHolder.contentContainer.Children())
        {
            float childLeft = child.layout.x;
            float childRight = child.layout.x + child.layout.width;

            // Si une partie de l'élément est visible dans la zone de scroll
            if (childRight > scrollX && childLeft < scrollX + viewWidth)
            {
                return child;
            }
        }

        return null;
    }

    private void CheckScoring(int key, int nbrDiscPlayed)
    {
        if(key < 1) return;

        //TODO: check scoring for the disc that just ended
        TextContent.Dialogue dialogue = TextContent.instance.FindKeyframe(key);
        SnapshotDisc.DiscData scene = DisksManager.instance.GetSceneParameters();

        int nbrErrors = 0;

        //rideau
        if (dialogue.rideau == "Fermé" && scene.curtainOpening == 1)
        {
            nbrErrors += 13; //all error !!!!!
        }
        else
        {
            if(dialogue.rideau == "Ouvert" && scene.curtainOpening == 0)
            {
                nbrErrors++;
            }

            //spot
            if (dialogue.onOffSpot == "Allumé")
            {
                if (dialogue.onOffSpot != scene.hasSpot)
                {
                    nbrErrors += 5;
                }
                else
                {
                    if (LightManager.instance.spotColorDict[dialogue.couleurSpot] != scene.spotColor)
                    {
                        nbrErrors++;
                    }
                    if (dialogue.diametreSpot == "Petit" && scene.spotSize == 1
                        || dialogue.diametreSpot == "Grand" && scene.spotSize == 0)
                    {
                        nbrErrors++;
                    }
                    if (dialogue.placementGrille9 != scene.spotPlacement)
                    {
                        nbrErrors++;
                    }
                    if (dialogue.mouvement == "Fixe" && !(scene.spotMouvement == -1)
                        || dialogue.mouvement == "G 2" && scene.spotMouvement == 0
                        || dialogue.mouvement == "G 1" && scene.spotMouvement == 1
                        || dialogue.mouvement == "D 1" && scene.spotMouvement == 2
                        || dialogue.mouvement == "D 2" && scene.spotMouvement == 3)
                    {
                        nbrErrors++;
                    }
                }
            }
            else if (dialogue.onOffSpot != scene.hasSpot)
            {
                nbrErrors++;
            }

            //ambiance
            if (dialogue.intensiteAmb > 0 && dialogue.intensiteAmb != scene.ambIntensity)
            {
                nbrErrors++;
            }
            if (dialogue.couleurAmb != "null" && LightManager.instance.ambColorDict[dialogue.couleurAmb] != scene.ambColor)
            {
                nbrErrors++;
            }

            //decors
            if (dialogue.decorL1 != scene.decorsL1)
            {
                nbrErrors++;
            }
            if (dialogue.decorL2 != scene.decorsL2)
            {
                nbrErrors++;
            }
            if (dialogue.decorL3 != scene.decorsL3)
            {
                nbrErrors++;
            }

            //sound
            if (dialogue.sons != scene.soundType)
            {
                nbrErrors++;
            }
        }

        if(nbrDiscPlayed > 1)
        {
            nbrErrors += (nbrDiscPlayed - 1);
        }

        Debug.Log($"Key {key} - Disques joués: {nbrDiscPlayed} - Erreurs: {nbrErrors}");

        if(nbrErrors == 0)
        {
            AudioManager.Instance.PlaySoundByName("Great");
        }
        else if(nbrErrors <= 3)
        {
            AudioManager.Instance.PlaySoundByName("Good");
        }
        else
        {
            AudioManager.Instance.PlaySoundByName("Bad");
        }
    }
}
