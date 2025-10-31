using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIScript : MonoBehaviour
{
    private VisualElement tiradeHolder;
    [SerializeField] private VisualTreeAsset tiradeTemplate;
    private Button previousPage, nextPage;
    private Label pageNumberText;

    //private int tiradesByPage = 4;
    //private int totalPages = 20;
    //56 tirades total

    private List<VisualElement> allItems = new();
    private List<List<VisualElement>> pages = new();

    private int currentPage = 0;

    private void Start()
    {
        UIManager.instance.OnInitButtons += InitButtons;
    }

    private void InitButtons()
    {
        VisualElement root = UIManager.instance.rootElement.Q("script");
        previousPage = root.Q<Button>("previousPageButton");
        nextPage = root.Q<Button>("nextPageButton");
        pageNumberText = root.Q<Label>("pageNumberLabel");
        tiradeHolder = root.Q<VisualElement>("tiradeHolder");

        previousPage.clicked += PrevPage;
        nextPage.clicked += NextPage;

        for (int i = 1; i <= TextContent.instance.GetTotalDialogue(); i++)
        {
            VisualElement tiradeInstance = tiradeTemplate.Instantiate();

            TextContent.Dialogue dialogue = TextContent.instance.FindKeyframe(i);
            tiradeInstance.Q<Label>("IDLabel").text = dialogue.key.ToString();
            tiradeInstance.Q<Label>("RegieLabel").text = dialogue.noteDirector != "null" ? dialogue.noteDirector : "";
            tiradeInstance.Q<Label>("TiradeLabel").text = $"{dialogue.character} – {dialogue.dialogue}";

            allItems.Add(tiradeInstance);
        }

        GeneratePages();
    }

    private void GeneratePages()
    {
        pages.Clear();

        float maxHeight = tiradeHolder.resolvedStyle.height;
        float currentHeight = 0;
        List<VisualElement> currentList = new();

        foreach (var item in allItems)
        {
            tiradeHolder.Add(item);
            tiradeHolder.MarkDirtyRepaint();
        }
        UIManager.instance.rootElement.schedule.Execute(() =>
        {

            foreach (var item in allItems)
            {
                float itemHeight = item.resolvedStyle.height;
                if (float.IsNaN(itemHeight))
                    itemHeight = 100f;

                if (currentHeight + itemHeight > maxHeight && currentList.Count > 0)
                {
                    pages.Add(new List<VisualElement>(currentList));
                    currentList.Clear();
                    currentHeight = 0;
                }

                currentList.Add(item);
                currentHeight += itemHeight;
            }

            if (currentList.Count > 0)
                pages.Add(currentList);

            UpdatePage();

        }).StartingIn(0);
    }

    private void UpdatePage()
    {
        tiradeHolder.Clear();

        if (pages.Count == 0)
            return;

        foreach (var item in pages[currentPage])
            tiradeHolder.Add(item);

        pageNumberText.text = $"{currentPage + 1} / {pages.Count}";
        previousPage.SetEnabled(currentPage > 0);
        nextPage.SetEnabled(currentPage < pages.Count - 1);
    }

    private void PrevPage()
    {
        if (currentPage <= 0) return;
        currentPage--;
        UpdatePage();
    }

    private void NextPage()
    {
        if (currentPage >= pages.Count - 1) return;
        currentPage++;
        UpdatePage();
    }    
}
