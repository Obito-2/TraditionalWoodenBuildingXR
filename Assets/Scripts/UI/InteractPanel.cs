using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InteractPanel : BaseFadePanel
    {
        private ModelInteraction _modelInteraction;
        private Button[] _buttonsList;
  
        private Text Modelinfo;
        
        // Start is called before the first frame update
        private void Start()
        {
            _modelInteraction = this.transform.root.GetComponent<ModelInteraction>();
            if (_modelInteraction == null)
            {
                Debug.LogError($"There is no {nameof(ModelInteraction)}.");
            }
            _buttonsList = transform.GetComponentsInChildren<Button>();
            foreach (var button in _buttonsList)
            {
             button.onClick.AddListener(() => ButtonOnClick(button));   
            }

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
                    _modelInteraction.ResPawnModel();
                    break;
                case "Anchor":
                    PanelAnchor();
                    break;
            } 
        }
        private void PanelAnchor()
        {
            Debug.LogWarning(" Interact Panel Anchor");
        }
    }
    
}
