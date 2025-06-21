using System;
using CodeArchitect.Manager.Event;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Build.Pipeline.Interfaces;
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

        protected override void Awake()
        {
            base.Awake();
            anchorToggle = transform.Find("Anchor").GetComponent<Toggle>();
            
            Modelinfo = transform.Find("ModelInfo").transform.GetComponent<TextMeshProUGUI>();
            
            _buttonsList = transform.GetComponentsInChildren<Button>();
            foreach (var button in _buttonsList)
            {
                button.onClick.AddListener(() => ButtonOnClick(button));   
            }
            EventCenter.Instance.AddListener<GameObject>(EventName.PieceSelected,ShowModelInfo);
            anchorToggle.onValueChanged.AddListener(OnAnchorToggleValueChanged);
            
        }

        // Start is called before the first frame update
        private void Start()
        {
            _modelInteraction = this.transform.root.GetComponent<ModelInteraction>();
            _interactableManager = this.transform.root.GetComponent<InteractableManager>();
            
            //todo:自动点击，测试代码，需要删除
            Button jigsawButton = transform.Find("Jigsaw").GetComponent<Button>();
            if (SceneManager.GetActiveScene().name == "DouGong")
            {
                // StartCoroutine(MyTools.DelayClickButton(jigsawButton));
            }
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
                
                case"Jigsaw":
                    Main.Instance.LoadSceneAsync("DouGongJigsaw");
                    break;
            } 
        }
        private void OnAnchorToggleValueChanged(bool value)
        {
            if (_interactableManager == null)
            {
                Debug.LogError("Interactable manager is null");
                return;
            }
            _interactableManager.IsModelAnchor(value);
        }

        private void ShowModelInfo(GameObject model)
        {
            Modelinfo.text = model.name;
        }

        protected override void OnDestroy()
        {
            EventCenter.Instance.RemoveListener<GameObject>(EventName.PieceSelected,ShowModelInfo);
        }

    }
    
}
