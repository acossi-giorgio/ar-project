using UnityEngine;
using UnityEngine.UI;

public class CardPager : MonoBehaviour
{
    [Header("Content pages")]
    public GameObject[] pages;

    [Header("Arrow buttons")]
    public GameObject btnPrev;
    public GameObject btnNext;

    [Header("Indicators (optional)")]
    public Image[] dots;

    [Header("Indicator colors")]
    public Color dotActive   = Color.white;
    public Color dotInactive = new Color(1f, 1f, 1f, 0.35f);

    private int _currentIndex = 0;

    private void Start() {
        ShowPage(_currentIndex);
    }

    public void NextPage()
    {
        if (_currentIndex < pages.Length - 1)
        {
            _currentIndex++;
            ShowPage(_currentIndex);
        }
    }

    public void PrevPage()
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            ShowPage(_currentIndex);
        }
    }

    private void ShowPage(int index)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == index);
        }

        for (int i = 0; i < dots.Length; i++)
        {
            dots[i].color = (i == index) ? dotActive : dotInactive;
        }

        UpdateArrowVisibility();
    }

    private void UpdateArrowVisibility()
    {
        if (btnPrev != null)
            btnPrev.SetActive(_currentIndex > 0);

        if (btnNext != null)
            btnNext.SetActive(_currentIndex < pages.Length - 1);
    }
}
