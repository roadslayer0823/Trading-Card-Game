using UnityEngine;
using System.Collections;
using TMPro;

public class FirstTurnIntro : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text announcementText;

    private float flashDuration = 1.5f;
    private float finalDisplayDuration = 1.0f;

    public IEnumerator FlashAndShowResult(bool isPlayerFirst)
    {
        float timer = 0f;
         this.gameObject.SetActive(true);

        while(timer < flashDuration)
        {
            announcementText.text = Random.value > 0.5f ? "PLAYER FIRST" : "ENEMY FIRST";
            announcementText.color = new Color(1f, 0.95f, 0.4f);

            yield return new WaitForSeconds(0.11f);
            timer += 0.11f;
        }

        if (isPlayerFirst)
        {
            announcementText.text = "PLAYER GOES FIRST!";
            announcementText.color = new Color(0.4f, 0.85f, 1f);
        }
        else
        {
            announcementText.text = "ENEMY GOES FIRST!";
            announcementText.color = new Color(1f, 0.35f, 0.35f);
        }

        yield return new WaitForSeconds(finalDisplayDuration);
        this.gameObject.SetActive(false);
    }
}
