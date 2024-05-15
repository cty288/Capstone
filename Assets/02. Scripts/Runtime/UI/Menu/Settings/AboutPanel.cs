using System;
using MikroFramework.UIKit;
using UnityEngine;



namespace Mikrocosmos
{
	public partial class AboutPanel : MikroUIPanel {
        [SerializeField] private float autoScrollSpeed = 1f;
        public override void OnInit() {
            
        }

        private void Update() {
            // if (IsOpening) {
            //     ScrollContent.verticalNormalizedPosition -= autoScrollSpeed * Time.deltaTime;
            //     ScrollContent.verticalNormalizedPosition = Mathf.Clamp01(ScrollContent.verticalNormalizedPosition);
            // }
            
        }

        private void Awake() {
            
        }

        public override void OnOpen(UIMsg msg) {
            ScrollContent.verticalNormalizedPosition = 1;
        }

        public override void OnClosed() {
            ScrollContent.verticalNormalizedPosition = 1;
        }
    }
}
