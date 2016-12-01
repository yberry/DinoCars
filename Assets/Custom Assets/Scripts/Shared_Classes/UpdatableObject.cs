using UnityEngine;
using System.Collections;
using System;

public class UpdatableObject : ScriptableObject, IUpdatable
{
	public virtual void OnFixedUpdate()
	{
		//throw new NotImplementedException();
	}

	public virtual void OnLateUpdate()
	{
	//	throw new NotImplementedException();
	}

	public virtual void OnUpdate()
	{
		//throw new NotImplementedException();
	}
}

public class SubscribableObject : UpdatableObject, ISubscribable
{

	public virtual void Subscribe(IUpdater updater)
	{
		updater.AddSubscriber(this);
	}

	public virtual void Unsubscribe(IUpdater updater)
	{
		updater.RemoveSubscriber(this);
	}

}