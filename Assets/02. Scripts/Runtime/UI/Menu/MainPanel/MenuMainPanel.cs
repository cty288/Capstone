using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.UIKit;
using Runtime.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuMainPanel : AbstractPanelContainer {
	[SerializeField] private Button continueGameButton;
	[SerializeField] private Button newGameButton;
	[SerializeField] private Button optionsButton;
	[SerializeField] private Button creditsButton;
	[SerializeField] private Button exitGameButton;

	private void Awake() {
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Time.timeScale = 1;
		ClientInput.Singleton.EnablePlayerMaps(false);
	}

	public override void OnInit() {
		continueGameButton.onClick.AddListener(OnContinueGameButtonClicked);
		newGameButton.onClick.AddListener(OnNewGameButtonClicked);
		optionsButton.onClick.AddListener(OnOptionsButtonClicked);
		creditsButton.onClick.AddListener(OnCreditsButtonClicked);
		exitGameButton.onClick.AddListener(OnExitGameButtonClicked);
	}

	private void OnExitGameButtonClicked() {
		Application.Quit();
	}

	private void OnCreditsButtonClicked() {
		
	}

	private void OnOptionsButtonClicked() {
		
	}

	private void OnNewGameButtonClicked() {
		if (HasSaveFile()) {
			UIManager.Singleton.Open<NewGameConfirmationPanel>(this, null);
		}
		else {
			EnterGame();
		}
	}

	private void OnContinueGameButtonClicked() {
		EnterGame();
	}

	public override void OnOpen(UIMsg msg) {
		continueGameButton.gameObject.SetActive(HasSaveFile());
	}

	public override void OnClosed() {
		
	}

	public static void EnterGame() {
		LoadingCanvas.Singleton.Show(() => {
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		}, 5f);

	}
	
	

	private bool HasSaveFile() {
		return ES3.FileExists("models_main.es3");
	}
}
