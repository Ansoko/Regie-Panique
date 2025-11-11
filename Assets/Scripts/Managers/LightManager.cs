using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static DisksManager;

public class LightManager : MonoBehaviour
{
    [SerializeField] private Light spot;
    [SerializeField] private Light ambiance;
    [SerializeField] private List<Transform> positions;

    [SerializeField] private List<Color> spotColors;
    [SerializeField] private List<Color> ambiantColors;
    [SerializeField] private const int maxAmbianceIntensity = 8000;

    public Dictionary<string, int> spotColorDict = new()
    {
        { "Blanc", 0 },
        { "Rouge", 1 },
        { "Bleu", 2 },
        { "Vert", 3 },
        { "Violet", 4 },
        { "Jaune", 5 },
        { "Orange", 6 },
        { "Marron", 7 },
    };

    public Dictionary<string, int> ambColorDict = new()
    {
        { "Blanc", 0 },
        { "Orange", 1 },
        { "Rouge", 2 },
        { "Violet", 3 }
    };

    int currentSpotPosition = 4;

    private Toggle spotOnOff;
    private ToggleButtonGroup spotPositions;
    private ToggleButtonGroup spotDirection;
    private ToggleButtonGroup spotColor;
    private Toggle spotDimension;
    private SliderInt ambianceIntensity;
    private VisualElement ambIntensityValue;
    private ToggleButtonGroup ambianceColor;

    [SerializeField] private Sprite noIntensity;
    [SerializeField] private Sprite Int0;
    [SerializeField] private Sprite Int25;
    [SerializeField] private Sprite Int50;
    [SerializeField] private Sprite Int75;
    [SerializeField] private Sprite Int100;

    public static LightManager instance;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        UIManager.instance.OnInitButtons += InitButtons;
        DisksManager.instance.OnPlayDisk += PlayDisc;
        DisksManager.instance.OnCreateDisk += RetreaveParameters;
    }

    private IEnumerator MoveSpot(float time, int direction)
    {
        Vector2Int deplacement = Vector2Int.zero;
        switch (direction)
        {
            case 3: //>>
                deplacement = Vector2Int.right * 2;
                break;
            case 2: //>
                 deplacement = Vector2Int.right * 1;
                break;
            case 1: //<
                deplacement = Vector2Int.left * 1;
                break;
            case 0: //<<
                deplacement = Vector2Int.left * 2;
                break;
        }

        Vector2Int coord = IndexToCoord(currentSpotPosition) + deplacement;
        Vector3 targetPosition;
        if (coord.x < 0) //déborde à gauche
        {
            currentSpotPosition = CoordToIndex(Vector2Int.up * coord.y);
            targetPosition = positions[currentSpotPosition].position + Vector3.left * Mathf.Abs(coord.x) * 30;
        }
        else if(coord.x > 2) //déborde à droite
        {
            currentSpotPosition = CoordToIndex(Vector2Int.right*2 + Vector2Int.up*coord.y);
            targetPosition = positions[currentSpotPosition].position + Vector3.right * Mathf.Abs(coord.x-2) * 30;
        }
        else
        {
            int newIndex = CoordToIndex(coord);
            currentSpotPosition = newIndex;
            targetPosition = positions[newIndex].position;
        }

        float elipsedTime = 0f;
        Quaternion startingRota = spot.transform.rotation;
        Quaternion targetRota = Quaternion.LookRotation(targetPosition - spot.transform.position);
        while (elipsedTime < time)
        {
            spot.transform.rotation = Quaternion.Slerp(startingRota, targetRota, elipsedTime / time);
            elipsedTime += Time.deltaTime;
            yield return null;
        }

        //todo : animation pour revenir à une position "stable"
        spot.transform.LookAt(positions[currentSpotPosition]);

    }

    Vector2Int IndexToCoord(int i)
    {
        int x = i % 3;
        int y = i / 3;
        return new Vector2Int(x, y);
    }

    int CoordToIndex(Vector2Int coord)
    {
        return coord.y * 3 + coord.x;
    }

    private int GetPositionSpot(ToggleButtonGroupState positionIndex)
    {
        var selected = positionIndex.GetActiveOptions(stackalloc int[positionIndex.length]);
        if (selected.Length <= 0)
            return -1;

        //disposition selected :
        //2-1-0
        //5-4-3
        //8-7-6

        //We want :
        // 6-7-8
        // 3-4-5
        // 0-1-2 (ou depuis 1 jsplus ????)
        return selected[0] switch
        {
            0 => 8,
            1 => 7,
            2 => 6,
            3 => 5,
            5 => 3,
            6 => 2,
            7 => 1,
            8 => 0,
            _ => selected[0],
        };
    }

    private int GetDirectionSpot(ToggleButtonGroupState positionIndex)
    {
        var selected = positionIndex.GetActiveOptions(stackalloc int[positionIndex.length]);
        if (selected.Length <= 0)
            return -1;
        return selected[0]; //0=<<, 1=<, 2=>, 3=>>
    }

    private int GetDimensionSpot(bool big) // false=small, true=big
    {
        //Quand faut il ne pas modifier le spot ?
        return big ? 1 : 0;
    }

    private int GetColorSpot(ToggleButtonGroupState indexColor)
    {
        var selected = indexColor.GetActiveOptions(stackalloc int[indexColor.length]);
        if (selected.Length <= 0)
            return -1;
        return selected[0];
    }

    private int GetColorAmbiance(ToggleButtonGroupState indexColor)
    {
        var selected = indexColor.GetActiveOptions(stackalloc int[indexColor.length]);
        if (selected.Length <= 0)
            return -1;
        return selected[0];
    }

    private int GetIntensityAmbiance(int intensity)
    {
        switch(intensity)
        {
            case 0:
                return 0;
            case 1:
                return maxAmbianceIntensity / 4;
            case 2:
                return maxAmbianceIntensity / 2;
            case 3:
                return (maxAmbianceIntensity * 3) / 4;
            case 4:
                return maxAmbianceIntensity;
            default:
                return -1;
        }
    }

    private string GetActivationSpot(bool value)
    {
        //is inverted
        return value ? "Éteint" : "Allumé";
    }

    private void PlayDisc(Disk currentDisc)
    {
        SnapshotDisc.DiscData currentDiscData = currentDisc.data;

        if (currentDiscData.hasSpot == "Éteint")
            spot.enabled = false;
        else if (currentDiscData.hasSpot == "Allumé")
            spot.enabled = true;

        if (currentDiscData.spotColor != -1)
            spot.color = spotColors[currentDiscData.spotColor];
        if (currentDiscData.spotSize != -1)
        {
            switch (currentDiscData.spotSize == 1)
            {
                case true:
                    spot.spotAngle = 45;
                    spot.innerSpotAngle = 40;
                    break;

                case false:
                    spot.spotAngle = 15;
                    spot.innerSpotAngle = 13;
                    break;
            }
        }
        if (currentDiscData.spotPlacement != -1)
        {
            spot.transform.LookAt(positions[currentDiscData.spotPlacement]);
            currentSpotPosition = currentDiscData.spotPlacement;
        }
        if(currentDiscData.spotMouvement > -1)
            StartCoroutine(MoveSpot(1, currentDiscData.spotMouvement));

        if (currentDiscData.ambIntensity != -1)
            ambiance.intensity = currentDiscData.ambIntensity;
        if (currentDiscData.ambColor != -1)
            ambiance.color = ambiantColors[currentDiscData.ambColor];
    }

    private void RetreaveParameters(Disk currentDisc)
    {
        SnapshotDisc.DiscData data = SnapshotDisc.DiscData.CreateDefault();
        data.hasSpot = GetActivationSpot(spotOnOff.value);
        if(data.hasSpot == "Allumé")
        {
            data.spotSize = GetDimensionSpot(spotDimension.value);
        }
        data.spotPlacement = GetPositionSpot(spotPositions.value);
        data.spotMouvement = GetDirectionSpot(spotDirection.value);
        data.spotColor = GetColorSpot(spotColor.value);

        data.ambIntensity = GetIntensityAmbiance(ambianceIntensity.value);
        data.ambColor = GetColorAmbiance(ambianceColor.value);

        ResetButtons();

        currentDisc.AddDataToDisk(data);
    }

    public SnapshotDisc.DiscData GetLightsFromScene()
    {
        SnapshotDisc.DiscData data = SnapshotDisc.DiscData.CreateDefault();
        data.hasSpot = spot.enabled ? "Allumé" : "Éteint";

        if(data.hasSpot == "Allumé")
        {
            data.spotSize = spot.spotAngle == 45 ? 1 : 0;
            data.spotColor = spotColors.IndexOf(spot.color);
            data.spotPlacement = currentSpotPosition;
        }

        data.ambIntensity = (int)ambiance.intensity switch
        {
            maxAmbianceIntensity => 100,
            (maxAmbianceIntensity * 3) / 4 => 75,
            maxAmbianceIntensity / 2 => 50,
            maxAmbianceIntensity / 4 => 25,
            0 => 0,
            _ => -1,
        };
        if (ambiance.intensity > 0)
            data.ambColor = ambiantColors.IndexOf(ambiance.color);

        return data;
    }

    private void InitButtons()
    {
        VisualElement root = UIManager.instance.rootElement.Q("lights");
        spotOnOff = root.Q<Toggle>("spotOnOff");
        spotPositions = root.Q<ToggleButtonGroup>("spotPad");
        spotDirection = root.Q<ToggleButtonGroup>("spotDirection");
        spotDimension = root.Q<Toggle>("spotDimension");
        spotColor = root.Q<ToggleButtonGroup>("spotCouleur");
        ambianceColor = root.Q<ToggleButtonGroup>("ambiantCouleur");
        ambianceIntensity = root.Q<SliderInt>("ambiantIntensity");

        ambianceIntensity.RegisterValueChangedCallback(evt =>
        {
            ambIntensityValue = root.Q<VisualElement>("ambIntensityValues");
            ambIntensityValue.style.backgroundImage = evt.newValue switch //between -1 and 4
            {
                0 => new StyleBackground(Int0),
                1 => new StyleBackground(Int25),
                2 => new StyleBackground(Int50),
                3 => new StyleBackground(Int75),
                4 => new StyleBackground(Int100),
                _ => new StyleBackground(noIntensity),
            };
        });
    }

    private void ResetButtons()
    {
        ClearGroup(spotPositions);
        ClearGroup(spotDirection);
        ClearGroup(spotColor);
        ClearGroup(ambianceColor);
        //spotOnOff.value = true;
        ambianceIntensity.value = -1;
    }

    private void ClearGroup(ToggleButtonGroup group)
    {
        int buttonCount = group.value.length;
        group.value = new ToggleButtonGroupState(0, buttonCount);
    }
}
