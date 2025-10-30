using UnityEngine;

public class SnapshotDisc : MonoBehaviour
{
    public struct DiscData
    {
        //Spot
        public string hasSpot;
        public int spotColor;
        public float spotSize;
        public int spotPlacement;
        public int spotMouvement;
        //Ambiance
        public int ambColor;
        public int ambIntensity;
        //Decors
        public string decorsL1;
        public string decorsL2;
        public string decorsL3;
        //Sound
        public string soundType;
        //Curtain
        public int curtainOpening;

        public static DiscData CreateDefault()
        {
            DiscData data = new DiscData
            {
                hasSpot = null,
                spotColor = -1,
                spotSize = -1,
                spotPlacement = -1,
                spotMouvement = -1,
                ambColor = -1,
                ambIntensity = -1,
                decorsL1 = null,
                decorsL2 = null,
                decorsL3 = null,
                soundType = null,
                curtainOpening = -1
            };
            return data;
        }
    }

    public float time = 1;
    [SerializeField] private MeshRenderer diskRenderer;
    public DiscData data;

    private void Start()
    {
        RandomColor();
    }

    public void Reset()
    {
        time = 1f;
        data.hasSpot = null;
        data.spotColor = -1;
        data.spotSize = -1;
        data.spotPlacement = -1;
        data.spotMouvement = -1;
        data.ambColor = -1;
        data.ambIntensity = -1;
        data.decorsL1 = null;
        data.decorsL2 = null;
        data.decorsL3 = null;
        data.soundType = null;
        data.curtainOpening = -1;
    }

    private void RandomColor()
    {
        if(diskRenderer == null) return;
        diskRenderer.material.color = Random.ColorHSV(0, 1, .9f, 1, .6f, 1);
    }

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
