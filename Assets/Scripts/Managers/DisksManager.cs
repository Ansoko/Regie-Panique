using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static SnapshotDisc;
using Random = UnityEngine.Random;

public class DisksManager : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset templateCardElement;

    [Header("Physique simulée")]
    public float moveSpeed = 50f;
    public float friction = 0.98f;
    public float repelForce = 100f;
    public float boundaryBounce = 0.5f;
    public float simulationInterval = 0.016f;

    private VisualElement root;
    private List<Disk> disks = new();
    private Coroutine physicsCoroutine;
    private Disk currentDisk;

    public Action<Disk> OnCreateDisk;
    public Action<Disk> OnPlayDisk;

    public static DisksManager instance;
    private void Awake()
    {
        instance = this;
    }

    public class Disk
    {
        public VisualElement element;
        public Vector2 velocity;
        public bool isDragging;
        public Vector2 dragOffset;
        public SnapshotDisc.DiscData data;

        public void AddDataToDisk(DiscData newData)
        {
            if (newData.hasSpot != null)
                data.hasSpot = newData.hasSpot;
            if (newData.spotColor != -1)
                data.spotColor = newData.spotColor;
            if (newData.spotSize != -1)
                data.spotSize = newData.spotSize;
            if (newData.spotPlacement != -1)
                data.spotPlacement = newData.spotPlacement;
            if (newData.spotMouvement != -1)
                data.spotMouvement = newData.spotMouvement;
            if (newData.ambColor != -1)
                data.ambColor = newData.ambColor;
            if (newData.ambIntensity != -1)
                data.ambIntensity = newData.ambIntensity;
            if (newData.decorsL1 != null)
                data.decorsL1 = newData.decorsL1;
            if (newData.decorsL2 != null)
                data.decorsL2 = newData.decorsL2;
            if (newData.decorsL3 != null)
                data.decorsL3 = newData.decorsL3;
            if (newData.soundType != null)
                data.soundType = newData.soundType;
            if (newData.curtainOpening != -1)
                data.curtainOpening = newData.curtainOpening;
        }
    }

    void Start()
    {
        UIManager.instance.OnInitButtons += InitButtons;
        root = UIManager.instance.rootElement;
    }

    /*
    private IEnumerator PhysicsLoop()
    {
        var wait = new WaitForSeconds(simulationInterval);

        while (disks.Count > 0)
        {
            StepPhysics(simulationInterval);
            yield return wait;
        }
    }

    private void StepPhysics(float deltaTime)
    {
        float maxX = root.resolvedStyle.width;
        float maxY = root.resolvedStyle.height;

        foreach (var disk in disks)
        {
            if (disk.isDragging)
                continue;

            Vector2 pos = new Vector2(disk.element.resolvedStyle.left, disk.element.resolvedStyle.top);
            pos += (disk.velocity * deltaTime);
            disk.velocity *= friction;


            disk.element.style.left = pos.x;
            disk.element.style.top = pos.y;
        }
    }
    */


    private void OnPointerDown(PointerDownEvent evt, Disk disk)
    {
        disk.isDragging = true;
        disk.dragOffset = evt.localPosition;
        disk.velocity = Vector2.zero;
        disk.element.CapturePointer(evt.pointerId);
        disk.element.pickingMode = PickingMode.Ignore;
        currentDisk = disk;

        AudioManager.Instance.PlaySoundByName("TakeDisk");
    }

    private void OnPointerMove(PointerMoveEvent evt, Disk disk)
    {
        if (!disk.isDragging) return;
        
        Vector2 newPos = (Vector2)evt.position - disk.dragOffset;
        disk.element.style.left = newPos.x;
        disk.element.style.top = newPos.y;
        disk.velocity = evt.deltaPosition;
    }

    private void OnPointerUp(PointerUpEvent evt, Disk disk)
    {
        disk.isDragging = false;
        disk.element.ReleasePointer(evt.pointerId);

        if (TimelineManager.instance.PlaceDiskOnTimeline(currentDisk, evt.position))
        {
            HideDisk(currentDisk);

            AudioManager.Instance.PlaySoundByName("PlaceTimeline");
        }
        else
        {
            AudioManager.Instance.PlaySoundByName("PlaceDisk");
        }

        currentDisk = null;
        disk.element.pickingMode = PickingMode.Position;
    }

    private void OnDisable()
    {
        if (physicsCoroutine != null)
            StopCoroutine(physicsCoroutine);
    }

    public void ShowDisk(Disk disc, bool select=true)
    {
        disc.element.style.display = DisplayStyle.Flex;
        disks.Add(disc);

        if (select)
        {
            var evt = PointerDownEvent.GetPooled();
            disc.isDragging = true;
            disc.dragOffset = evt.localPosition;
            disc.velocity = Vector2.zero;
            disc.element.CapturePointer(evt.pointerId);
            currentDisk = disc;
            AudioManager.Instance.PlaySoundByName("TakeDisk");
        }
    }

    private void HideDisk(Disk disc)
    {
        disc.element.style.display = DisplayStyle.None;
        disks.Remove(disc);
    }

    private void InitButtons()
    {
        Button createButton = UIManager.instance.rootElement.Q<Button>("CreateDisk");
        createButton.clicked += CreateDisk;
    }

    private void CreateDisk()
    {
        var disk = new Disk();
        disk.data = SnapshotDisc.DiscData.CreateDefault();
        disk.element = templateCardElement.Instantiate();
        disk.element.Q<VisualElement>("insideCard").style.unityBackgroundImageTintColor = Random.ColorHSV(0, 1, .9f, 1, .6f, 1);
        disk.element.style.left = 1708;
        disk.element.style.top = 723;

        root.Add(disk.element);

        disk.element.RegisterCallback<PointerDownEvent>(evt => OnPointerDown(evt, disk));
        disk.element.RegisterCallback<PointerMoveEvent>(evt => OnPointerMove(evt, disk));
        disk.element.RegisterCallback<PointerUpEvent>(evt => OnPointerUp(evt, disk));

        disks.Add(disk);

        OnCreateDisk?.Invoke(disk);
        AudioManager.Instance.PlaySoundByName("CreateDisk");
    }

    public void PlayDisk(Disk currentDisc)
    {
        OnPlayDisk?.Invoke(currentDisc);
    }

    public SnapshotDisc.DiscData GetSceneParameters()
    {
        Disk data = new();

        data.AddDataToDisk(LightManager.instance.GetLightsFromScene());
        data.AddDataToDisk(DecorsManager.instance.GetDecorFromScene());

        return data.data;
    }

}
