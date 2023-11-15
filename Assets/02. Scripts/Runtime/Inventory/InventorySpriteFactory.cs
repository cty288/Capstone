using Framework;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using UnityEngine;
using UnityEngine.U2D;

namespace Runtime.Inventory {
	public class InventorySpriteFactory : ISingleton, ICanGetUtility {
		private ResLoader resLoader;
		private SpriteAtlas atlas;
		public static InventorySpriteFactory Singleton {
			get { return SingletonProperty<InventorySpriteFactory>.Singleton; }
		}

		private InventorySpriteFactory(){}
		public void OnSingletonInit() {
			resLoader = this.GetUtility<ResLoader>();
			atlas = resLoader.LoadSync<SpriteAtlas>("entities/resources_inventory", $"ResourceInvAtlas");
		}

		public Sprite GetSprite(string entityname) {
			Sprite sprite = atlas.GetSprite($"{entityname}_Icon");
			return sprite;
		}
		

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}