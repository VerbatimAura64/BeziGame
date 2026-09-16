using UnityEngine;
using Harborview.GameTools;
using UnityEngine.UI;
using TMPro;

public class FM : MonoBehaviour, IFragmentDisplay
{
    public GameObject panel;
    public TextMeshProUGUI text;

    public GameObject Panel => panel;

    void IFragmentDisplay.Unlock(StoryFragment fragment) => Unlock(fragment);
    void IFragmentDisplay.ClosePanel() => TogglePanel();

    public void TogglePanel() => Panel.SetActive(!Panel.activeInHierarchy);
    //bool panelActive = (panel.activeInHierarchy) ? panel.SetActive(false)  : panel.SetActive(true);


    public void Unlock(StoryFragment fragment)
    {
        Panel.SetActive(true);
        text.text = fragment.fragmentText;

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
