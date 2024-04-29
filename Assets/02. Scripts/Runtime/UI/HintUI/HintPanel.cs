using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.UIKit;
using Polyglot;
using Runtime.Controls;
using Runtime.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


public enum HintMessageType {
	NPCDialogue,
	Panel,
	Subtitle
}

[Serializable]
public class HintMessageGroup {
	public HintMessageType messageType;
	public HintMessage[] messages;
}

[Serializable]
public class HintMessage {
	public string messageLocalizedKey;
	public string[] keyParameters;
	public string titleLocalizedKey;
	public Sprite icon;
	public float duration;
	[SerializeField] public UnityEvent callback;
}
public abstract class HintPanel : AbstractPanelContainer, IController, IGameUIPanel {
	protected HintMessageGroup currentMessageGroup = null;
	public HintMessageGroup CurrentMessageGroup => currentMessageGroup;


	protected int currentMessageIndex;
	private HashSet<Action<HintPanel>> onPanelClose = new HashSet<Action<HintPanel>>();
	[SerializeField] private bool canCloseWithButton = false;
	private bool canClose = false;
	
	public void StartNewMessageGroup(HintMessageGroup messageGroup) {
		if (currentMessageGroup != null) {
			TerminateCurrentMessageGroup();
		}
		currentMessageGroup = messageGroup;
		currentMessageIndex = 0;
		OnShowMessage();
	}

	protected string GetLocalizedText(HintMessage message) {
		string text = Localization.Get(message.messageLocalizedKey);
		if(message.keyParameters != null && message.keyParameters.Length > 0) {
			InputAction[] acts = new InputAction[message.keyParameters.Length];
			for (int i = 0; i < message.keyParameters.Length; i++) {
				acts[i] = ClientInput.Singleton.FindActionInPlayerActionMap(message.keyParameters[i]);
			}
			object[] localizedKeys = new object[message.keyParameters.Length];
			for (int i = 0; i < message.keyParameters.Length; i++) {
				localizedKeys[i] = ControlInfoFactory.Singleton.GetBindingKeyLocalizedName(acts[i]);
			}

			text = Localization.GetFormat(message.messageLocalizedKey, localizedKeys);
		}

		return text;
	}
	
	public void RegisterOnPanelClose(Action<HintPanel> action) {
		onPanelClose.Add(action);
	}

	public void UnregisterOnPanelClose(Action<HintPanel> action) {
		onPanelClose.Remove(action);
	}
	

	public void TerminateCurrentMessageGroup() {
		if (currentMessageGroup == null) {
			return;
		}
		
		OnTerminateCurrentMessageGroup(currentMessageIndex == currentMessageGroup.messages.Length);
		currentMessageGroup = null;
	}

	protected abstract void OnTerminateCurrentMessageGroup(bool isLastMessage);
	protected abstract void OnShowMessage();

	public void ShowNextMessage() {
		if (currentMessageGroup == null) {
			return;
		}
		HintMessageGroup messageGroupTemp = this.currentMessageGroup;
		int lastIndex = currentMessageIndex;
		
		if (currentMessageIndex < currentMessageGroup.messages.Length - 1) {
			currentMessageIndex++;
			OnShowMessage();
		}
		else {
			canClose = true;
			MainUI.Singleton.GetAndClose(this);
		}

		var lastHintMessage = lastIndex >= 0 ? messageGroupTemp.messages[lastIndex] : null;
		if (lastHintMessage != null && lastHintMessage.callback != null) {
			lastHintMessage.callback.Invoke();
		}
	}
	
	public void ShowLastMessage() {
		if (currentMessageGroup == null) {
			return;
		}
		if (currentMessageIndex > 0) {
			currentMessageIndex--;
			OnShowMessage();
		}
	}

	public override void OnInit() {
		
	}

	public override void OnOpen(UIMsg msg) {
		
	}

	public override void OnClosed() {
		
		foreach (var action in onPanelClose) {
			action(this);
		}
		TerminateCurrentMessageGroup();
		canClose = false;
	}

	public bool CanCloseByEscButton { get; } = false;

	public IArchitecture GetArchitecture() {
		return MainGame.Interface;
	}

	public IPanel GetClosePanel() {
		if (canCloseWithButton) {
			return this;
		}else {
			return canClose ? this : null;
		}
	}
}
