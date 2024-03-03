using _02._Scripts.Runtime.Levels.Models;
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
		private ILevelModel levelModel;
		protected override void Awake() {
			base.Awake();
			controlActions = ClientInput.Singleton.GetSharedActions();
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			playerModel = this.GetModel<IGamePlayerModel>();
			levelModel = this.GetModel<ILevelModel>();
			ClientInput.Singleton.EnablePlayerMaps();
			this.RegisterEvent<OnOpenPillarUI>(OnOpenPillarUI)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		}

		private void OnOpenPillarUI(OnOpenPillarUI e) {
			OpenOrGetClose<PillarUIViewController>(this, e, false);
			
		}

		private void Update() {
			
			if (controlActions.Close.WasPressedThisFrame()) {
				if (currentMainPanel != null) {
					//ClosePanel(currentMainPanel);
					//OpenOrGetClose(currentMainPanel, null);
					//Time = 1;
					GetAndClose(currentMainPanel);
					//ClientInput.Singleton.EnablePlayerMaps();
				}
			}
			

			if (playerModel.IsPlayerDead()) {
				return;
			}
			
			if (!levelModel.IsInBase() && controlActions.Inventory.WasPressedThisFrame() && (currentMainPanel == null || UIManager.Singleton.GetPanel<InventoryUIViewController>(true))) {
				OpenOrGetClose<InventoryUIViewController>(this, null, true);
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

		/// <summary>
		/// Use this Open
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="message"></param>
		/// <param name="switchUIPlayerMap"></param>
		/// <param name="createNewIfNotExist"></param>
		/// <param name="assetNameIfNotExist"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Open<T>(IPanelContainer parent, UIMsg message, bool isPopup, bool switchUIPlayerMap = true,
			bool createNewIfNotExist = true, string assetNameIfNotExist = "") where T : class, IPanel {
			if (currentMainPanel != null && !isPopup) {
				ClosePanel(currentMainPanel);
			}
		
			if (switchUIPlayerMap) {
				ClientInput.Singleton.EnableUIMaps();
				
			}
			
			//Time.timeScale = 0;
			return Open<T>(parent, message, createNewIfNotExist, assetNameIfNotExist);
		}

		public void GetAndClose(IPanel panel, bool switchUIPlayerMap = true, bool alsoCloseChild = true) {
			IPanel closePanel = GetToClosePanel(panel);
			if (closePanel != null) {
				ClosePanel(closePanel, alsoCloseChild);
				if (switchUIPlayerMap) {
					if (currentMainPanel == null) {
						ClientInput.Singleton.EnablePlayerMaps();
					}
				}
			}
		}
		
		
		public T OpenOrGetClose<T>(IPanelContainer parent, UIMsg message, bool isPopup, bool switchUIPlayerMap = true, 
			bool createNewIfNotExist = true, string assetNameIfNotExist = "")  where T : class, IPanel {
			if (currentMainPanel!=null && currentMainPanel.GetType() == typeof(T)) {
				GetAndClose(currentMainPanel, switchUIPlayerMap);
				return null;
			}


			return Open<T>(parent, message,  isPopup, switchUIPlayerMap, createNewIfNotExist, assetNameIfNotExist);
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
		
		protected IPanel GetToClosePanel(IPanel panel) {
			if (panel == null) {
				return null;
			}
			
			if(panel is IGameUIPanel gameUIPanel) {
				return gameUIPanel.GetClosePanel();
			}
			
			return panel;
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}

		public void OnSingletonInit() {
			
		}
		
		public static MainUI Singleton => SingletonProperty<MainUI>.Singleton;
	}
}
