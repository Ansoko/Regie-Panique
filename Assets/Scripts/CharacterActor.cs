using System.Collections;
using UnityEngine;

public class CharacterActor : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text dialogText;

    public void Speak(string dialog)
    {
        dialogText.text = $"<mark=#000000>{dialog}</mark>";
    }

    public void ClearDialog()
    {
        dialogText.text = "";
    }

    public void MoveTo(Vector3 position, float duration)
    {
        if(duration <= 0f)
        {
            transform.position = position;
            return;
        }
        StartCoroutine(MoveOverTime(position, duration));
    }

    private IEnumerator MoveOverTime(Vector3 targetPosition, float duration)
    {
        AudioManager.Instance.PlaySoundByName("Bruit de pas (Dev)");
        Vector3 startPosition = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        AudioManager.Instance.StopSFX("Bruit de pas (Dev)");
        transform.position = targetPosition;
    }
}
