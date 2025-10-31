using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DecorsManager : MonoBehaviour
{
    [Header("Decors")]
    [SerializeField] private List<string> nameDecorsL1 = new();
    [SerializeField] private List<Sprite> listDecorsL1 = new();
    [SerializeField] private List<GameObject> GameObjectDecorsL1 = new();
    [SerializeField] private List<string> nameDecorsL2 = new();
    [SerializeField] private List<Sprite> listDecorsL2 = new();
    [SerializeField] private List<GameObject> GameObjectDecorsL2 = new();
    [SerializeField] private List<string> nameDecorsL3 = new();
    [SerializeField] private List<Sprite> listDecorsL3 = new();
    [SerializeField] private List<GameObject> GameObjectDecorsL3 = new();
    [SerializeField] private Transform decorParent;
    private int currentIndexL1 = 0;
    private int currentIndexL2 = 0;
    private int currentIndexL3 = 0;
    private GameObject currentDecorL1, currentDecorL2, currentDecorL3;

    [Header("Curtain")]
    [SerializeField] private GameObject leftCurtain;
    [SerializeField] private GameObject rightCurtain;
    [SerializeField] private SpriteRenderer curtainLeftSide, curtainRightSide;
    [SerializeField] private float curtainClosedPositionX = 33.25f;
    [SerializeField] private float curtainOpenPositionX = 74f;
    [SerializeField] private Sprite openCurtainSprite, closedCurtainSprite;
    [SerializeField] private float durationCurtainAnimation = 2f;

    private Button previousDecorsL1, nextDecorsL1;
    private Button previousDecorsL2, nextDecorsL2;
    private Button previousDecorsL3, nextDecorsL3;
    private VisualElement imgDecorsL1, imgDecorsL2, imgDecorsL3;
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
        AudioManager.Instance.PlaySoundByName("rideaux", 1);
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
        AudioManager.Instance.PlaySoundByName("rideaux", 1);
        StartCoroutine(CloseCurtains());
    }


    private void RetreaveParameters(SnapshotDisc currentDisc)
    {
        SnapshotDisc.DiscData data = SnapshotDisc.DiscData.CreateDefault();
        if (currentIndexL1 > 0)
            data.decorsL1 = nameDecorsL1[currentIndexL1];
        if (currentIndexL2 > 0)
            data.decorsL2 = nameDecorsL2[currentIndexL2];
        if (currentIndexL3 > 0)
            data.decorsL3 = nameDecorsL3[currentIndexL3];
        data.curtainOpening = curtainToggle.value ? 1 : 0;

        ResetButtons();

        currentDisc.AddDataToDisk(data);
    }

    private void InitButtons()
    {
        VisualElement root = UIManager.instance.rootElement.Q("decors");
        nextDecorsL1 = root.Q<Button>("nextDecorsL1");
        previousDecorsL1 = root.Q<Button>("previousDecorsL1");
        imgDecorsL1 = root.Q<VisualElement>("imgDecorsL1");
        nextDecorsL2 = root.Q<Button>("nextDecorsL2");
        previousDecorsL2 = root.Q<Button>("previousDecorsL2");
        imgDecorsL2 = root.Q<VisualElement>("imgDecorsL2");
        nextDecorsL3 = root.Q<Button>("nextDecorsL3");
        previousDecorsL3 = root.Q<Button>("previousDecorsL3");
        imgDecorsL3 = root.Q<VisualElement>("imgDecorsL3");

        nextDecorsL1.clicked += NextDecorL1;
        previousDecorsL1.clicked += PreviousDecorL1;
        nextDecorsL2.clicked += NextDecorL2;
        previousDecorsL2.clicked += PreviousDecorL2;
        nextDecorsL3.clicked += NextDecorL3;
        previousDecorsL3.clicked += PreviousDecorL3;

        curtainToggle = UIManager.instance.rootElement.Q<Toggle>("curtainOpening");
    }

    private void NextDecorL1()
    {
        currentIndexL1++;
        if (currentIndexL1 >= listDecorsL1.Count)
            currentIndexL1 = 0;
        imgDecorsL1.style.backgroundImage = listDecorsL1[currentIndexL1] != null ? new StyleBackground(listDecorsL1[currentIndexL1]) : null;
    }
    private void PreviousDecorL1()
    {
        currentIndexL1--;
        if (currentIndexL1 < 0)
            currentIndexL1 = listDecorsL1.Count - 1;
        imgDecorsL1.style.backgroundImage = listDecorsL1[currentIndexL1] != null ? new StyleBackground(listDecorsL1[currentIndexL1]) : null;
    }
    private void NextDecorL2()
    {
        currentIndexL2++;
        if (currentIndexL2 >= listDecorsL2.Count)
            currentIndexL2 = 0;
        imgDecorsL2.style.backgroundImage = listDecorsL2[currentIndexL2] != null ? new StyleBackground(listDecorsL2[currentIndexL2]) : null;
    }
    private void PreviousDecorL2()
    {
        currentIndexL2--;
        if (currentIndexL2 < 0)
            currentIndexL2 = listDecorsL2.Count - 1;
        imgDecorsL2.style.backgroundImage = listDecorsL2[currentIndexL2] != null ? new StyleBackground(listDecorsL2[currentIndexL2]) : null;
    }
    private void NextDecorL3()
    {
        currentIndexL3++;
        if (currentIndexL3 >= listDecorsL3.Count)
            currentIndexL3 = 0;
        imgDecorsL3.style.backgroundImage = listDecorsL3[currentIndexL3] != null ? new StyleBackground(listDecorsL3[currentIndexL3]) : null;
    }
    private void PreviousDecorL3()
    {
        currentIndexL3--;
        if (currentIndexL3 < 0)
            currentIndexL3 = listDecorsL3.Count - 1;
        imgDecorsL3.style.backgroundImage = listDecorsL3[currentIndexL2] != null ? new StyleBackground(listDecorsL3[currentIndexL3]) : null;
    }

    private void ResetButtons()
    {
        currentIndexL1 = 0;
        imgDecorsL1.style.backgroundImage = new StyleBackground(listDecorsL1[0]);
        currentIndexL2 = 0;
        imgDecorsL2.style.backgroundImage = new StyleBackground(listDecorsL2[0]);
        currentIndexL3 = 0;
        imgDecorsL3.style.backgroundImage = new StyleBackground(listDecorsL3[0]);
    }

    private void PlayDisc(SnapshotDisc currentDisc)
    {
        if(currentDisc.data.decorsL1 != null)
        {
            Destroy(currentDecorL1);
            int indexL1 = nameDecorsL1.IndexOf(currentDisc.data.decorsL1);
            Instantiate(GameObjectDecorsL1[indexL1], decorParent);
        }
        if (currentDisc.data.decorsL2 != null)
        {
            Destroy(currentDecorL2);
            int indexL2 = nameDecorsL2.IndexOf(currentDisc.data.decorsL2);
            Instantiate(GameObjectDecorsL2[indexL2], decorParent);
        }
        if (currentDisc.data.decorsL3 != null)
        {
            Destroy(currentDecorL3);
            int indexL3 = nameDecorsL3.IndexOf(currentDisc.data.decorsL3);
            Instantiate(GameObjectDecorsL3[indexL3], decorParent);
        }

        if (currentDisc.data.curtainOpening == 1)
            TriggerOpenCurtains();
        else if (currentDisc.data.curtainOpening == 0)
            TriggerCloseCurtains();
    }
}
