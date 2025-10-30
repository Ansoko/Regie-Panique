using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class DisksManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    private GameObject diskSelected;
    [SerializeField] private float mouseDragPhysicsSpeed = 10;
    [SerializeField] private float heightDiskDrag = 0.8f;
    [SerializeField] private float heightDiskPlaced = 0.8f;
    private bool pressed = false;

    [SerializeField] private SnapshotDisc templateDisc;
    [SerializeField] private Transform disksParent;

    public Action<SnapshotDisc> OnCreateDisk;
    public Action<SnapshotDisc> OnPlayDisk;

    public static DisksManager instance;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        UIManager.instance.OnInitButtons += InitButtons;
        InputSystem.actions.FindAction("Click").started += SelectDisk;
        InputSystem.actions.FindAction("Click").canceled += ReleaseDisk;
    }

    private void SelectDisk(InputAction.CallbackContext ctx)
    {
        Vector2 screenpos = Mouse.current.position.value;
        Vector3 camdis = new Vector3(screenpos.x, screenpos.y, mainCamera.transform.position.y);
        Ray ray = mainCamera.ScreenPointToRay(camdis);
        if (Physics.Raycast(ray, out RaycastHit hit))
            if (hit.collider != null && hit.collider.attachedRigidbody != null)
            {
                Rigidbody rb = hit.collider.attachedRigidbody;
                SelectDisk(rb.gameObject, hit.collider);
                return;
            }

        diskSelected = null;
    }

    private void SelectDisk(GameObject disc, Collider col)
    {
        diskSelected = disc.gameObject;
        pressed = true;
        diskSelected.transform.rotation = Quaternion.Euler(Vector3.up*180);
        col.enabled = false;
        AudioManager.Instance.PlaySoundByName("TakeDisk");

        StartCoroutine(MoveDisk());
    }

    private IEnumerator MoveDisk()
    {
        if (diskSelected == null) yield break;
        diskSelected.TryGetComponent<Rigidbody>(out var rb);
        if (rb == null) yield break;

        do
        {
            diskSelected.transform.localPosition = new Vector3(diskSelected.transform.localPosition.x, heightDiskDrag, diskSelected.transform.localPosition.z);

            Vector3 direction = cursorOnTransform - diskSelected.transform.position;
            rb.linearVelocity = direction * mouseDragPhysicsSpeed;

            yield return null;
        }
        while (pressed);
    }

    private Vector3 cursorWorldPosOnNCP
    {
        get
        {
            Vector2 mousePosition = InputSystem.actions.FindAction("Point").ReadValue<Vector2>();
            return Camera.main.ScreenToWorldPoint(
                new Vector3(mousePosition.x,
                mousePosition.y,
                Camera.main.nearClipPlane));
        }
    }

    private Vector3 cameraToCursor
    {
        get
        {
            return cursorWorldPosOnNCP - Camera.main.transform.position;
        }
    }

    private Vector3 cursorOnTransform
    {
        get
        {
            Vector3 camToTrans = diskSelected.transform.position - Camera.main.transform.position;
            return Camera.main.transform.position +
                cameraToCursor *
                (Vector3.Dot(Camera.main.transform.forward, camToTrans) / Vector3.Dot(Camera.main.transform.forward, cameraToCursor));
        }
    }

    private void ReleaseDisk(InputAction.CallbackContext ctx)
    {
        if (diskSelected == null) return;

        if (TimelineManager.instance.currentTarget != null)
        {
            TimelineManager.instance.PlaceDiskOnTimeline(diskSelected.GetComponent<SnapshotDisc>());

            HideDisk(diskSelected);

            AudioManager.Instance.PlaySoundByName("PlaceTimeline");
        }
        else
        {
            diskSelected.TryGetComponent<Collider>(out var col);
            col.enabled = true;
            diskSelected.TryGetComponent<Rigidbody>(out var rb);
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
            Vector3 pos = diskSelected.transform.localPosition;
            pos.y = heightDiskPlaced;
            diskSelected.transform.localPosition = pos;

            AudioManager.Instance.PlaySoundByName("PlaceDisk");
        }

        diskSelected = null;
        pressed = false;
    }

    public void ShowDisk(SnapshotDisc disc)
    {
        disc.gameObject.SetActive(true);
        var col = disc.GetComponent<Collider>();
        SelectDisk(disc.gameObject, col);
    }

    private void HideDisk(GameObject disc)
    {
        disc.SetActive(false);
    }

    private void InitButtons()
    {
        Button createButton = UIManager.instance.rootElement.Q<Button>("CreateDisk");
        createButton.clicked += CreateDisk;
    }

    private void CreateDisk()
    {
        SnapshotDisc disk = Instantiate(templateDisc, disksParent);
        disk.Reset();
        OnCreateDisk?.Invoke(disk);
        AudioManager.Instance.PlaySoundByName("CreateDisk");
    }

    public void PlayDisk(SnapshotDisc currentDisc)
    {
        OnPlayDisk?.Invoke(currentDisc);
    }

}
