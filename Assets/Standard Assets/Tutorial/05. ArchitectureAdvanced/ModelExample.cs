using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

public interface IModelExample : IModel {
	
}

public class ModelExample : IModelExample {
	protected IArchitecture architecture;
	public IArchitecture GetArchitecture() {
		return this.architecture;
	}

	public void SetArchitecture(IArchitecture architecture) {
		this.architecture = architecture;
	}

	public void Init() {
		
	}
}
