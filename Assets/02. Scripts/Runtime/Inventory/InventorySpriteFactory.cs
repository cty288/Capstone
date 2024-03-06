﻿using Framework;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
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
			string iconName = ResourceTemplates.Singleton.GetResourceTemplates(entityname).TemplateEntity.GetIconName();
			Sprite sprite = atlas.GetSprite(iconName);
			return sprite;
		}

		public Sprite GetSprite(IResourceEntity resourceEntity) {
			Sprite sprite = atlas.GetSprite(resourceEntity.GetIconName());
			return sprite;
		}


		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}