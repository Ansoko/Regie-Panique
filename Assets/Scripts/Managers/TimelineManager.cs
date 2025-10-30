using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TimelineManager : MonoBehaviour
{
    public float speed = 1f;
    [SerializeField] private List<float> durationTimestamp;
    [SerializeField] private VisualTreeAsset timelineTemplate;
    private ScrollView timelineHolder;
    private float totalDuration = 0;
    private float widthTimeline = 0;
    public VisualElement currentTarget;
    private float elipsedTime = 0f;

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
        durationTimestamp.Add(10); //key 0 n'existe pas
        foreach (TextContent.Dialogue dialogue in TextContent.instance.timestamps)
        {
            if (dialogue.key < 1) continue;

            if (dialogue.character == "Rideau")
            {
                durationTimestamp.Add(10);
                totalDuration += 10;
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
        timelineHolder.contentContainer.style.width = 150 * totalDuration;
        widthTimeline = 150 * totalDuration;
        for (int i = 0; i < durationTimestamp.Count; i++)
        {
            VisualElement timelineInstance = timelineTemplate.Instantiate();
            VisualElement rootElement = timelineInstance.Q("case");
            rootElement.style.width = durationTimestamp[i] * 150;

            rootElement.RegisterCallback<MouseEnterEvent>(HoverCase);
            rootElement.RegisterCallback<MouseLeaveEvent>(UnhoverCase);

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
        timelineCases[currentTarget.parent] = snapshotDisc;
        if (currentTarget != null)
            currentTarget.style.backgroundColor = Color.green;
    }

    private IEnumerator PlayTimeline()
    {
        elipsedTime = 0f;
        while (elipsedTime < totalDuration)
        {
            timelineHolder.scrollOffset = new Vector2((elipsedTime / totalDuration) * widthTimeline, 0);

            VisualElement firstVisible = GetFirstVisibleElement();
            if (firstVisible != null && currentPlayingDisc != timelineCases[firstVisible])
            {
                if (currentPlayingDisc != null)
                {
                    //TODO: check scoring for the disc that just ended
                }
                //Destroy(currentPlayingDisc?.gameObject);
                currentPlayingDisc = timelineCases[firstVisible];
                if (currentPlayingDisc != null)
                {
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
}
