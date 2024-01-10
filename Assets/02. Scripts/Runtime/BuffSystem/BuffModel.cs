using System;
using System.Collections.Generic;
using Framework;


namespace _02._Scripts.Runtime.BuffSystem {
	public interface IBuffModel : ISavableModel {
		BuffModelContainer BuffModelContainer { get; }
	}

	
	public class BuffModel : AbstractSavableModel, IBuffModel {
		[ES3Serializable]
		private BuffModelContainer buffModelContainer = new BuffModelContainer();

		protected override void OnInit() {
			base.OnInit();
			if (!IsFirstTimeCreated) {
				buffModelContainer.InitFromSave();
			}
		}

		public BuffModelContainer BuffModelContainer => buffModelContainer;
	}
}