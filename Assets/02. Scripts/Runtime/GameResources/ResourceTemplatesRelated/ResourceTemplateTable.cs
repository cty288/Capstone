using MikroFramework.DataStructures;
using Runtime.GameResources.Model.Base;

namespace Runtime.GameResources {
	public class ResourceTemplateTable : Table<ResourceTemplateInfo> {
		public TableIndex<string, ResourceTemplateInfo> NameIndex { get; private set; }
		public TableIndex<ResourceCategory, ResourceTemplateInfo> CategoryIndex { get; private set; } 

		public ResourceTemplateTable() {
			NameIndex = new TableIndex<string, ResourceTemplateInfo>
				(item => item.TemplateEntity.EntityName);

			CategoryIndex = new TableIndex<ResourceCategory, ResourceTemplateInfo>(info =>
				info.TemplateEntity.GetResourceCategory());
		}

		protected override void OnClear() {
			NameIndex.Clear();
			CategoryIndex.Clear();
			
		}

		public override void OnAdd(ResourceTemplateInfo item) {
			NameIndex.Add(item);
			CategoryIndex.Add(item);
		}

		public override void OnRemove(ResourceTemplateInfo item) {
			NameIndex.Remove(item);
			CategoryIndex.Remove(item);
		}
	}
	

}