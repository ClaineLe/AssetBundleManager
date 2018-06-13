
using UnityEngine;
using System.Collections;

public abstract class ManagerBase<T> :MonoSingleton<T> where T : MonoBehaviour, IManager
{
	public virtual IEnumerator Initialize(){
		yield break;
	}

	public override void Release ()
	{
		base.Release ();
	}
}
