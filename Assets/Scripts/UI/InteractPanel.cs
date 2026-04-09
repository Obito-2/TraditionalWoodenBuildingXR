using System;
using CodeArchitect.Manager.Event;
using TMPro;
using Unity.VisualScripting;
// using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class InteractPanel : BaseFadePanel
    {
        private ModelInteraction _modelInteraction;
        private InteractableManager _interactableManager;
        
        [SerializeField]
        private Button[] _buttonsList;
        private TextMeshProUGUI Modelinfo;
        private Toggle anchorToggle;
        private Button AIChatButton;
        
        [SerializeField] private Sprite onSprite;     // Toggle 打开时的图片
        [SerializeField] private Sprite offSprite;    // Toggle 关闭时的图片
        
        protected override void Awake()
        {
            base.Awake();
            anchorToggle = transform.Find("Anchor").GetComponent<Toggle>();
            
            Modelinfo = transform.Find("ModelInfo").transform.GetComponent<TextMeshProUGUI>();
            
            _buttonsList = transform.GetComponentsInChildren<Button>();
            foreach (var button in _buttonsList)
            {
                var btn = button; // 避免闭包问题
                button.onClick.AddListener(() => ButtonOnClick(btn));
                // 找到 AIChat 按钮
                if (button.name == "AIChat")
                {
                    AIChatButton = button;
                    AIChatButton.gameObject.SetActive(false); // 初始隐藏该按钮
                    AIChatButton.interactable = false;
                }
            }
            
            EventCenter.Instance.AddListener<GameObject>(EventName.PieceSelected,ShowModelInfo);
            anchorToggle.onValueChanged.AddListener(OnAnchorToggleValueChanged);
            
        }

        // Start is called before the first frame update
        private void Start()
        {
            _modelInteraction = this.transform.root.GetComponent<ModelInteraction>();
            _interactableManager = this.transform.root.GetComponent<InteractableManager>();
            
            // //todo:自动点击，测试代码，需要删除
            // Button AIChatButton = transform.Find("AIChat").GetComponent<Button>();
            // if (SceneManager.GetActiveScene().name == "DouGong")
            // {
            //     StartCoroutine(MyTools.DelayClickButton(AIChatButton));
            //     Debug.LogError("自动测试：点击AICHAT按钮");
            // }
        }
        private void ButtonOnClick(Button button)
        {
            switch (button.name)
            {
                case "Combination":
                    if (_modelInteraction == null)
                    {Debug.LogError("找不到_modelInteraction");
                        return;
                    }
                    _modelInteraction.CombinationModel();
                    break;
                
                case "Explode":
                    if (_modelInteraction == null)
                    {Debug.LogError("找不到_modelInteraction");
                        return;
                    }
                    _modelInteraction.ExplodeModel();
                    break;
                
                case "Reset":
                    Main.Instance.RespawnModel(this.transform.root.gameObject.name);
                    break;
                
                case "AIChat":
                    //首先加载dialogue
                    UI3DManager.Instance.ShowPanelOnSpecificCanvas<DialoguePanel>(nameof(DialoguePanel), this.transform.parent, (panel) =>
                    {
                        //触发事件，发送当前模型信息作为query
                        EventCenter.Instance.TriggerEvent(EventName.AIChat,Modelinfo.text);
                        
                    });
                    break;
            } 
        }
        //anchor按钮控制是否可以交互
        private void OnAnchorToggleValueChanged(bool value)
        {
            if (_interactableManager == null)
            {
                Debug.LogError("Interactable manager is null");
                return;
            }
            _interactableManager.IsModelAnchor(value);
            anchorToggle.image.sprite = value ? onSprite : offSprite;
            
        }

        private void ShowModelInfo(GameObject model)
        {
            Modelinfo.text = model.name;
            if (AIChatButton != null)
            {
                AIChatButton.gameObject.SetActive(true);
                AIChatButton.interactable = true;
            }
        }

        protected override void OnDestroy()
        {
            EventCenter.Instance.RemoveListener<GameObject>(EventName.PieceSelected,ShowModelInfo);
            anchorToggle.onValueChanged.RemoveListener(OnAnchorToggleValueChanged);
        }

    }
    
}
