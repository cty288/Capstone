using Framework;
using MikroFramework.Architecture;
using MikroFramework.Singletons;
using MikroFramework.UIKit;
using Runtime.Controls;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.Player;
using Runtime.Spawning.Commands;
using Runtime.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace Runtime.UI {
	public class MainUI : UIRoot, IController, ISingleton {
		DPunkInputs.SharedActions controlActions;
		private IGamePlayerModel playerModel;
		protected override void Awake() {
			base.Awake();
			controlActions = ClientInput.Singleton.GetSharedActions();
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			playerModel = this.GetModel<IGamePlayerModel>();
			this.RegisterEvent<OnOpenPillarUI>(OnOpenPillarUI)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		}

		private void OnOpenPillarUI(OnOpenPillarUI e) {
			OpenOrClose<PillarUIViewController>(this, e);
		}

		private void Update() {
			
			if (controlActions.Close.WasPressedThisFrame()) {
				if (currentMainPanel != null) {
					ClosePanel(currentMainPanel);
					//Time = 1;
					ClientInput.Singleton.EnablePlayerMaps();
				}
			}
			

			if (playerModel.IsPlayerDead()) {
				return;
			}
			
			if (controlActions.Inventory.WasPressedThisFrame()) {
				OpenOrClose<InventoryUIViewController>(this, null, true);
			}

			/*if (Input.GetKeyDown(KeyCode.I)) {
				IInventoryModel inventoryModel = this.GetModel<IInventoryModel>();
				inventoryModel.AddSlots(2);
				inventoryModel.AddHotBarSlots(HotBarCategory.Left, 1, ()=>new LeftHotBarSlot());
			}*/
		}
		


		public override T Open<T>(IPanelContainer parent, UIMsg message, bool createNewIfNotExist = true, string assetNameIfNotExist = "") {
			T panel = base.Open<T>(parent, message, createNewIfNotExist, assetNameIfNotExist);
			if (currentMainPanel != null) {
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				Time.timeScale = 0;
			}
			//ClientInput.Singleton.EnableUIMaps();
			return panel;
		}
	
		public T OpenOrClose<T>(IPanelContainer parent, UIMsg message, bool switchUIPlayerMap = true, 
			bool createNewIfNotExist = true, string assetNameIfNotExist = "")  where T : class, IPanel {
			if (currentMainPanel!=null && currentMainPanel.GetType() == typeof(T)) {
				ClosePanel(currentMainPanel);
				if (switchUIPlayerMap) {
					ClientInput.Singleton.EnablePlayerMaps();
				}

				//Time.timeScale = 1;
				return null;
			}
			
			if (currentMainPanel != null) {
				ClosePanel(currentMainPanel);
			}
		
			if (switchUIPlayerMap) {
				ClientInput.Singleton.EnableUIMaps();
			}
			
			//Time.timeScale = 0;
			return Open<T>(parent, message, createNewIfNotExist, assetNameIfNotExist);
		}

		public override void ClosePanel(IPanel panel, bool alsoCloseChild = true) {
			base.ClosePanel(panel, alsoCloseChild);
			if (currentMainPanel == null) {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				Time.timeScale = 1;
			}
			//ClientInput.Singleton.EnablePlayerMaps();
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}

		public void OnSingletonInit() {
			
		}
		
		public static MainUI Singleton => SingletonProperty<MainUI>.Singleton;
	}
}
