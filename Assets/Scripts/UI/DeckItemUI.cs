using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckItemUI : MonoBehaviour
{
    public TMP_Text nameText;
    public Image deckImage;
    public Button selectButton;

    public void Setup(string name, Sprite deckSprite = null)
    {
        nameText.text = name;
        deckImage.sprite = deckSprite;
    }
}
