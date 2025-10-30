using UnityEngine;
using UnityEngine.UIElements;

public class UIScript : MonoBehaviour
{
    private VisualElement tiradeHolder;
    [SerializeField] private VisualTreeAsset tiradeTemplate;
    private Button previousPage, nextPage;
    private Label pageNumberText;

    private int tiradesByPage = 4;
    //56 tirades total

    private int currentPageIndex = 0;
    private int totalPages = 20;

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

        previousPage.clicked += PreviousPage;
        nextPage.clicked += NextPage;

        SetPage(0);
    }

    private void PreviousPage()
    {
        if (currentPageIndex <= 0) return;
        currentPageIndex--;
        SetPage(currentPageIndex);
    }

    private void NextPage()
    {
        if(currentPageIndex >= totalPages - 1) return;
        currentPageIndex++;
        SetPage(currentPageIndex);
    }

    private void SetPage(int pageNumber)
    {
        tiradeHolder.Clear();
        for (int i = 0; i < tiradesByPage; i++)
        {
            VisualElement tiradeInstance = tiradeTemplate.Instantiate();

            TextContent.Dialogue dialogue = TextContent.instance.FindKeyframe(pageNumber * tiradesByPage + i + 1);
            tiradeInstance.Q<Label>("IDLabel").text = dialogue.key.ToString();
            tiradeInstance.Q<Label>("RegieLabel").text = "";
            tiradeInstance.Q<Label>("TiradeLabel").text = $"{dialogue.character} – {dialogue.dialogue}";
            //tiradeInstance.Q<Label>("IDLabel").text = TextContent.instance.GetDialogueById($"ref_{pageNumber * tiradesByPage + i + 1}");
            //tiradeInstance.Q<Label>("RegieLabel").text = TextContent.instance.GetDialogueById($"director_{pageNumber * tiradesByPage + i + 1}");
            //tiradeInstance.Q<Label>("TiradeLabel").text = TextContent.instance.GetDialogueById($"tirade_{pageNumber * tiradesByPage + i + 1}");

            tiradeHolder.Add(tiradeInstance);
        }
        
        pageNumberText.text = $"Page {pageNumber + 1} / {totalPages}";
        if(pageNumber == 0)
        {
            previousPage.SetEnabled(false);
        }
        else
        {
            previousPage.SetEnabled(true);
        }
        if (pageNumber == totalPages - 1)
        {
            nextPage.SetEnabled(false);
        }
        else
        {
            nextPage.SetEnabled(true);
        }
    }

}
