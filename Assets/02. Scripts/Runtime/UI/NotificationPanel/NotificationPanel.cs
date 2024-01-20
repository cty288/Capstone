using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Extensions;
using MikroFramework.Singletons;
using Runtime.UI.Crafting.Research;
using Runtime.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class NotificationInfo {
    public GameObject SpawnedNotification;
    public float Duration;

    public NotificationInfo(GameObject spawnedNotification, float duration) {
        SpawnedNotification = spawnedNotification;
        Duration = duration;
    }
}
public class NotificationPanel : MonoMikroSingleton<NotificationPanel>, IController {
    private Queue<NotificationInfo> notificationQueue = new Queue<NotificationInfo>();
    private NotificationInfo currentNotification = null;
    private RectTransform parentRectTransform;
    
    [SerializeField] private GameObject researchNotificationPrefab;
    
    

    private void Awake() {
        parentRectTransform = GetComponent<RectTransform>();
        this.RegisterEvent<OnResearchEventTriggered>(OnResearchEventTriggered).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
    }

    private void OnResearchEventTriggered(OnResearchEventTriggered e) {
        GameObject spawnedNotification = Instantiate(researchNotificationPrefab, parentRectTransform);
        ResearchNotificationPanel researchNotificationPanel = spawnedNotification.GetComponent<ResearchNotificationPanel>();
        
        List<string> entityNames = new List<string>();
        foreach (var result in e.ResearchResults) {
            if (result.ResearchedEntityNames == null) {
                continue;
            }
            entityNames.AddRange(result.ResearchedEntityNames);
        }

        researchNotificationPanel.SetContent(entityNames.ToHashSet().ToArray());
        
        AddNotification(spawnedNotification, 5f);
    }

    private void Update() {
        if(currentNotification == null && notificationQueue.Count > 0) {
            currentNotification = notificationQueue.Dequeue();
            
            GameObject spawnedNotification = currentNotification.SpawnedNotification;
            RectTransform rectTransform = spawnedNotification.GetComponent<RectTransform>();
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(rectTransform.DOAnchorPosY(rectTransform.rect.height, 0.5f).SetEase(Ease.OutBack));
            sequence.AppendInterval(currentNotification.Duration);
            sequence.Append(rectTransform.DOAnchorPosY(0, 0.5f).SetEase(Ease.InBack));
            sequence.OnComplete(() => {
                Destroy(spawnedNotification);
                currentNotification = null;
            });
        }
    }

    /// <summary>
    /// The spawned notification must be pivot at x=1, y=1; anchored at bottom right corner
    /// </summary>
    /// <param name="spawnedNotification"></param>
    /// <param name="duration"></param>
    public void AddNotification(GameObject spawnedNotification, float duration) {
        spawnedNotification.transform.SetParent(parentRectTransform);
        RectTransform rectTransform = spawnedNotification.GetComponent<RectTransform>();
        rectTransform.anchoredPosition= Vector2.zero;
        StartCoroutine(ShowNotification(spawnedNotification, duration));
    }
    
    private IEnumerator ShowNotification(GameObject spawnedNotification, float duration) {
        RectTransform rectTransform = spawnedNotification.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        notificationQueue.Enqueue(new NotificationInfo(spawnedNotification, duration));
    }

    public IArchitecture GetArchitecture() {
        return MainGame.Interface;
    }
}
