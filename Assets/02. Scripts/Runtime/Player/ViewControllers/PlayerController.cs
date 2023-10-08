using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Singletons;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Player;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.Temporary
{
    public class PlayerController : AbstractCreatureViewController<PlayerEntity>, ISingleton, ICanDealDamageViewController {
        private static HashSet<PlayerController> players = new HashSet<PlayerController>();
        private CameraShaker cameraShaker;

        protected override void Awake() {
            base.Awake();
            cameraShaker = GetComponentInChildren<CameraShaker>();
        }

        public static PlayerController GetClosestPlayer(Vector3 position) {
            if (players.Count == 0) {
                //try find gameobject of type playercontroller
                var playerController = GameObject.FindObjectsOfType<PlayerController>();
                if (playerController.Length == 0) {
                    return null;
                }
                players = new HashSet<PlayerController>(playerController);
            }
            
            
            PlayerController closestPlayer = null;
            float closestDistance = float.MaxValue;

            foreach (var player in players) {
                if (players.Count == 1) {
                    return player;
                }
                float distance = Vector3.Distance(player.transform.position, position);

                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }

            return closestPlayer;
        }
        protected override IEntity OnBuildNewEntity() {
            return this.GetModel<IGamePlayerModel>().GetPlayer();
        }

        protected override void OnEntityStart() {
            Debug.Log("PlayerController.OnEntityStart");
            players.Add(this);
        }

        protected override void OnBindEntityProperty() {
            
        }

        protected override void OnEntityDie(IBelongToFaction damagedealer) {
            
        }

        protected override void OnEntityTakeDamage(int damage, int currenthealth, IBelongToFaction damagedealer) {
            float damageRatio = Mathf.Clamp01((float)damage / (float)BoundEntity.GetMaxHealth());
            CameraShakeData shakeData = new CameraShakeData(
                Mathf.Lerp(0f, 3f, damageRatio),
                Mathf.Lerp(0.1f, 0.5f, damageRatio),
                 Mathf.RoundToInt(Mathf.Lerp(10f, 50f, damageRatio))
            );

            cameraShaker.Shake(shakeData, CameraShakeBlendType.Maximum);
        }

        protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer) {
            
        }

        public void OnSingletonInit() {
            
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            players.Remove(this);
        }

        public ICanDealDamage CanDealDamageEntity => BoundEntity;
    }
}
