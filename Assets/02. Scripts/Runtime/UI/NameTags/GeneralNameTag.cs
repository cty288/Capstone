using System.Collections;
using Runtime.DataFramework.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.NameTags {
    public interface INameTag{
        public void SetEntity(IEntity entity);
        
        public void SetName(string name);
        public void Refresh();
    }
    public class GeneralNameTag : MonoBehaviour, INameTag {
        protected TMP_Text nameText;
        protected RectTransform rectTr;

        protected virtual void Awake() {
            nameText = transform.Find("NameText").GetComponent<TMP_Text>();
            rectTr = GetComponent<RectTransform>();
        }

        public virtual void SetEntity(IEntity entity) {
            nameText.text = entity.GetDisplayName();
            Refresh();
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