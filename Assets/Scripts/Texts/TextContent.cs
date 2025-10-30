using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEditor.Progress;

public class TextContent : MonoBehaviour
{

    public struct Dialogue
    {
        public string acte;
        public string scene;
        public string titre;
        public int key;
        public string character;
        public string dialogue;
        public int newPosition;
        public string decorL3;
        public string decorL2;
        public string decorL1;
        public int intensiteAmb;
        public string couleurAmb;
        public string onOffSpot;
        public string couleurSpot;
        public string diametreSpot;
        public int placementGrille9;
        public string mouvement;
        public string sons;
        public string rideau;

        public Dialogue(string acte, string scene, string titre, int key, string character, string dialogue, int newPosition, string decorL3, string decorL2, string decorL1, int intensiteAmb, string couleurAmb, string onOffSpot, string couleurSpot, string diametreSpot, int placementGrille9, string mouvement, string sons, string rideau)
        {
            this.acte = acte;
            this.scene = scene;
            this.titre = titre;
            this.key = key;
            this.character = character;
            this.dialogue = dialogue;
            this.newPosition = newPosition;
            this.decorL3 = decorL3;
            this.decorL2 = decorL2;
            this.decorL1 = decorL1;
            this.intensiteAmb = intensiteAmb;
            this.couleurAmb = couleurAmb;
            this.onOffSpot = onOffSpot;
            this.couleurSpot = couleurSpot;
            this.diametreSpot = diametreSpot;
            this.placementGrille9 = placementGrille9;
            this.mouvement = mouvement;
            this.sons = sons;
            this.rideau = rideau;
        }
    }

    //private Dictionary<string, string> dictionnaire = new();
    public List<Dialogue> timestamps = new();
    public string csvFile;

    public static TextContent instance;
    private void Awake()
    {
        instance = this;
        LoadCSV();
    }

    void LoadCSV()
    {
        TextAsset textFile = Resources.Load<TextAsset>(csvFile);
        using StringReader reader = new StringReader(textFile.text);
        reader.ReadLine();

        while (true)
        {
            string line = reader.ReadLine();
            if (line == string.Empty || line == null)
                break;
            line = line.Replace(";;", ";null;");
            line = line.Replace(";;", ";null;");
            string[] values = line.Split(';');
            timestamps.Add(new Dialogue(values[0],
                                        values[1],
                                        values[2],
                                        Int32.Parse(values[3] != "null" ? values[3] : "-1"),
                                        values[4],
                                        values[5],
                                        Int32.Parse(values[6] != "null" ? values[6] : "0"),
                                        values[7],
                                        values[8],
                                        values[9],
                                        Int32.Parse(values[10] != "null" ? values[10] : "-1"),
                                        values[11],
                                        values[12],
                                        values[13],
                                        values[14],
                                        -1,//Int32.Parse(values[15] != "null" ? values[15] : "-1"),
                                        values[16],
                                        values[17],
                                        values[18]));
        }
    }

    public Dialogue FindKeyframe(int key)
    {
        foreach (Dialogue dialogue in timestamps)
        {
            if (dialogue.key == key)
            {
                return dialogue;
            }
        }
        return new();
    }

}
