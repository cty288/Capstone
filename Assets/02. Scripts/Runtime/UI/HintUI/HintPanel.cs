using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.UIKit;
using Runtime.UI;
using UnityEngine;


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
public struct HintMessage {
	[TextArea]
	public string message;
	public string title;
	public Sprite icon;
	public float duration;
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
		if (currentMessageIndex < currentMessageGroup.messages.Length - 1) {
			currentMessageIndex++;
			OnShowMessage();
		}
		else {
			canClose = true;
			MainUI.Singleton.GetAndClose(this);
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
