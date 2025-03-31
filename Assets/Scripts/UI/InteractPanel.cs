using System;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InteractPanel : BaseFadePanel
    {
        private ModelInteraction _modelInteraction;
        private JigsawInteraction _jigsawInteraction;
        [SerializeField]
        private Button[] _buttonsList;
  
        private Text Modelinfo;

        // Start is called before the first frame update
        private void Start()
        {
            _modelInteraction = this.transform.root.GetComponent<ModelInteraction>();
            _jigsawInteraction = this.transform.root.GetComponent<JigsawInteraction>();
            if (_modelInteraction == null || _jigsawInteraction == null)
            {
                Debug.LogError($"InteractPanel获取交互组件失败.");
            }
            _buttonsList = transform.GetComponentsInChildren<Button>();
            foreach (var button in _buttonsList)
            {
             button.onClick.AddListener(() => ButtonOnClick(button));   
            }
            Button jigsawButton = transform.Find("Jigsaw").GetComponent<Button>();
            StartCoroutine(MyTools.DelayClickButton(jigsawButton));
            
        }
        private void ButtonOnClick(Button button)
        {
            switch (button.name)
            {
                case "Combination":
                    _modelInteraction.CombinationModel();
                    break;
                case "Explode":
                    _modelInteraction.ExplodeModel();
                    break;
                case "Reset":
                    Main.Instance.RespawnModel(this.transform.root.gameObject.name);
                    break;
                case "Anchor":
                    PanelAnchor();
                    break;
                case"Jigsaw":
                    _jigsawInteraction.JigsawInitial();
                    break;
            } 
        }
        private void PanelAnchor()
        {
            Debug.LogWarning(" Interact Panel Anchor");
        }
    }
    
}
