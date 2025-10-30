using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DecorsManager : MonoBehaviour
{
    [Header("Decors")]
    [SerializeField] private List<string> nameDecors = new();
    [SerializeField] private List<GameObject> listDecors = new();

    [Header("Curtain")]
    [SerializeField] private GameObject leftCurtain;
    [SerializeField] private GameObject rightCurtain;
    [SerializeField] private SpriteRenderer curtainLeftSide, curtainRightSide;
    [SerializeField] private float curtainClosedPositionX = 33.25f;
    [SerializeField] private float curtainOpenPositionX = 74f;
    [SerializeField] private Sprite openCurtainSprite, closedCurtainSprite;
    [SerializeField] private float durationCurtainAnimation = 2f;

    private Toggle curtainToggle;

    private void Start()
    {
        UIManager.instance.OnInitButtons += InitButtons;
        DisksManager.instance.OnPlayDisk += PlayDisc;
        DisksManager.instance.OnCreateDisk += RetreaveParameters;
        TriggerOpenCurtains();
    }

    private IEnumerator OpenCurtains()
    {
        float elapsedTime = 0f;
        Vector3 leftStartPos = leftCurtain.transform.position;
        Vector3 rightStartPos = rightCurtain.transform.position;
        Vector3 leftEndPos = new Vector3(curtainOpenPositionX, leftStartPos.y, leftStartPos.z);
        Vector3 rightEndPos = new Vector3(-curtainOpenPositionX, rightStartPos.y, rightStartPos.z);
        curtainLeftSide.sprite = openCurtainSprite;
        curtainRightSide.sprite = openCurtainSprite;
        while (elapsedTime < durationCurtainAnimation)
        {
            leftCurtain.transform.position = Vector3.Lerp(leftStartPos, leftEndPos, (elapsedTime / durationCurtainAnimation));
            rightCurtain.transform.position = Vector3.Lerp(rightStartPos, rightEndPos, (elapsedTime / durationCurtainAnimation));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        leftCurtain.transform.position = leftEndPos;
        rightCurtain.transform.position = rightEndPos;
    }

    public void TriggerOpenCurtains()
    {
        StartCoroutine(OpenCurtains());
    }

    private IEnumerator CloseCurtains()
    {
        float elapsedTime = 0f;
        Vector3 leftStartPos = leftCurtain.transform.position;
        Vector3 rightStartPos = rightCurtain.transform.position;
        Vector3 leftEndPos = new Vector3(curtainClosedPositionX, leftStartPos.y, leftStartPos.z);
        Vector3 rightEndPos = new Vector3(-curtainClosedPositionX, rightStartPos.y, rightStartPos.z);
        curtainLeftSide.sprite = closedCurtainSprite;
        curtainRightSide.sprite = closedCurtainSprite;
        while (elapsedTime < durationCurtainAnimation)
        {
            leftCurtain.transform.position = Vector3.Lerp(leftStartPos, leftEndPos, (elapsedTime / durationCurtainAnimation));
            rightCurtain.transform.position = Vector3.Lerp(rightStartPos, rightEndPos, (elapsedTime / durationCurtainAnimation));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        leftCurtain.transform.position = leftEndPos;
        rightCurtain.transform.position = rightEndPos;
    }

    public void TriggerCloseCurtains()
    {
        StartCoroutine(CloseCurtains());
    }


    private void RetreaveParameters(SnapshotDisc currentDisc)
    {
        SnapshotDisc.DiscData data = SnapshotDisc.DiscData.CreateDefault();
        //data.decorsL1 = GetPositionSpot(spotPositions.value);
        data.curtainOpening = curtainToggle.value ? 1 : 0;

        ResetButtons();

        currentDisc.AddDataToDisk(data);
    }

    private void InitButtons()
    {
        VisualElement root = UIManager.instance.rootElement.Q("decors");
        //spotDirection = root.Q<ToggleButtonGroup>("spotDirection");

        curtainToggle = UIManager.instance.rootElement.Q<Toggle>("curtainOpening");
    }

    private void ResetButtons()
    {

    }

    private void PlayDisc(SnapshotDisc currentDisc)
    {
        if (currentDisc.data.curtainOpening == 1)
            TriggerOpenCurtains();
        else if (currentDisc.data.curtainOpening == 0)
            TriggerCloseCurtains();
    }
}
