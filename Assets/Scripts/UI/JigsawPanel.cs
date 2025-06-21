using System;
using CodeArchitect.Manager.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 面板脚本实例化滞后与其他脚本，因为是在Interaction脚本中进行加载的
/// </summary>
public class JigsawPanel : BaseFadePanel
{
    private JigsawInteraction _jigsawInteraction;
    private InteractableManager _interactableManager;

    [SerializeField] 
    private Button[] _buttonsList;
    private TextMeshProUGUI Modelinfo;
    [SerializeField]
    private Toggle[] toggles;
    private UIMenuDelayFollowHead canvasFollowHead;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        Modelinfo = transform.Find("ModelInfo").transform.GetComponent<TextMeshProUGUI>();

        _buttonsList = transform.GetComponentsInChildren<Button>();
        toggles = transform.GetComponentsInChildren<Toggle>();
        
        foreach (var button in _buttonsList)
        {
            button.onClick.AddListener(() => ButtonOnClick(button));   
        }
        foreach (var toggle in toggles)
        {
            toggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(toggle.name, isOn));
        }
        EventCenter.Instance.AddListener<GameObject>(EventName.PieceSelected,ShowModelInfo);

    }
    private void Start()
    {
        _interactableManager = transform.root.GetComponent<InteractableManager>();
        canvasFollowHead = transform.GetComponentInParent<UIMenuDelayFollowHead>();
    }

    private void ButtonOnClick(Button button)
    {
        switch (button.name)
        {
            case "Reset":
                Main.Instance.LoadSceneAsync("DouGongJigsaw");
                break;
            
            case"Return":
                Main.Instance.LoadSceneAsync("DouGong");
                Main.Instance.LoadModel("DouGong");
                break;
        } 
    }
    private void OnToggleValueChanged(String toggleName,bool value)
    {

        switch (toggleName)
        {
            case "Anchor":
                if (_interactableManager == null)
                {
                    Debug.LogError("Interactable manager is null");
                }
                _interactableManager.IsModelAnchor(value);
                break;
            
            case "PanelAnchor":
                if (canvasFollowHead == null)
                {
                    Debug.LogError("canvasFollowHead is null");
                }
                canvasFollowHead.enabled = !value;
                break;
        }
    }
    
    public void ShowModelInfo(GameObject model)
    {
        Modelinfo.text = model.name;
    }
    protected override void OnDestroy()
    {
        EventCenter.Instance.RemoveListener<GameObject>(EventName.PieceSelected,ShowModelInfo);
    }
    
}
