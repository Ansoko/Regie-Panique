public class SnapshotDisc
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

        public override readonly string ToString()
        {
            return $"Spot: [HasSpot: {hasSpot}, Color: {spotColor}, Size: {spotSize}, Placement: {spotPlacement}, Mouvement: {spotMouvement}] " +
                   $"Ambiance: [Color: {ambColor}, Intensity: {ambIntensity}] " +
                   $"Decors: [L1: {decorsL1}, L2: {decorsL2}, L3: {decorsL3}] " +
                   $"Sound: [Type: {soundType}] " +
                   $"Curtain: [Opening: {curtainOpening}]";
        }
    }
}
