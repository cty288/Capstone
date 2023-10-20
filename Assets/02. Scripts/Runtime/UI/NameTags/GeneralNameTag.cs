using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.NameTags {
    public interface INameTag{
        public void SetName(string name);
        public void Refresh();
    }
    public class GeneralNameTag : MonoBehaviour, INameTag {
        protected TMP_Text nameText;
        protected RectTransform rectTr;

        private void Awake() {
            nameText = transform.Find("NameText").GetComponent<TMP_Text>();
            rectTr = GetComponent<RectTransform>();
        }

        public void SetName(string name) {
            nameText.text = name;
            Refresh();
        }

        public void Refresh() {
            if (gameObject.activeInHierarchy) {
                StartCoroutine(RebuildLayout());
            }
        }
    
        private IEnumerator RebuildLayout() {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTr);
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTr);
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTr);
        }
    }
}