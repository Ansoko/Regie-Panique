using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.Rendering.DebugUI.Table;

public class ActorManager : MonoBehaviour
{
    [SerializeField] private CharacterActor mother;
    [SerializeField] private CharacterActor father;
    [SerializeField] private CharacterActor king;
    [SerializeField] private CharacterActor guard;
    [SerializeField] private CharacterActor bougrelas;
    [SerializeField] private CharacterActor narrator;

    private Dictionary<string, CharacterActor> actors = new();

    [Header("Positions")]
    private Dictionary<int, Transform> positions = new();
    [SerializeField] private List<int> indexPositions;
    [SerializeField] private List<Transform> transformPositions;

    private void Start()
    {
        actors.Add("Mère Ubu", mother);
        actors.Add("Père Ubu", father);
        actors.Add("Roi Venceslas", king);
        actors.Add("Capitaine Bordure", guard);
        actors.Add("Bougrelas", bougrelas);
        actors.Add("Narrateur", narrator);
        
        for (int i = 0; i < Mathf.Min(indexPositions.Count, transformPositions.Count); i++)
        {
            positions.Add(indexPositions[i], transformPositions[i]);
        }

        StartCoroutine(StartThePlay());
    }

    private IEnumerator StartThePlay()
    {
        foreach (TextContent.Dialogue dialogue in TextContent.instance.timestamps)
        {
            float duration = 0;
            if (dialogue.character == "Rideau")
            {
                duration = 10;
            }
            else if(dialogue.key >= 1)
            {
                duration = 1;
            }

            ActorSpeak(dialogue.character, dialogue.dialogue);

            if (dialogue.newPosition != 0)
            {
                actors[dialogue.character].MoveTo(positions[dialogue.newPosition].position, duration);
            }

            if (dialogue.key < 1)
                continue;
            yield return new WaitForSeconds(duration / TimelineManager.instance.speed);
        }
    }

    private void ActorSpeak(string character, string dialogue)
    {
        if (dialogue == "null") dialogue = ""; ;
        foreach (var actor in actors)
        {
            if(actor.Key != character)
                actor.Value.ClearDialog();
        }
        if(actors.ContainsKey(character))
        {
            actors[character].Speak(dialogue);
        }
    }
}
