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
    private Transform backCanvasTransform;
    
    [SerializeField] private Sprite onSprite;     // Toggle 打开时的图片
    [SerializeField] private Sprite offSprite;    // Toggle 关闭时的图片

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
        backCanvasTransform = transform.root.Find("BackCanvas");
        if (backCanvasTransform == null)
        {
            Debug.LogError("backCanvasTransform can't be found");
        }
        canvasFollowHead = backCanvasTransform.GetComponent<UIMenuDelayFollowHead>();
        
    }

    private void ButtonOnClick(Button button)
    {
        switch (button.name)
        {
            case "Reset":
                Main.Instance.LoadSceneAsync(ExperienceSession.JigsawSceneName);
                break;
            
            case "Return":
                Main.Instance.LoadHubAndShowMainMenu();
                break;
        } 
    }
    private void OnToggleValueChanged(String toggleName,bool value)
    {

        switch (toggleName)
        {
            case "Anchor"://模型整体是否可以抓取
                if (_interactableManager == null)
                {
                    Debug.LogError("Interactable manager is null");
                }
                _interactableManager.IsModelAnchor(value);
                break;
            
            //TODO: 关闭跟随同时不跟随父物体转动？跟随是否可以上下跟随？
            case "PanelAnchor"://构件面板是否跟随头部运动
                if (canvasFollowHead == null)
                {
                    Debug.LogError("canvasFollowHead is null");
                }
                Debug.Log("背板跟随 / 关闭跟随");
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
