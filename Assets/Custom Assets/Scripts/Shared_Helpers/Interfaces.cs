using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public interface ITimed
{
    float TimerDuration { get; }
}


public interface IUpdatable
{
	void OnFixedUpdate();
	void OnUpdate();
	void OnLateUpdate();
}

public interface ISubscribable : IUpdatable
{
	//IUpdater Updater { get; }
	void Subscribe(IUpdater updater);
	void Unsubscribe(IUpdater updater);
}

public interface IUpdater
{	
	void AddSubscriber(IUpdatable subscriber);
	void RemoveSubscriber(IUpdatable subscriber);
}

public interface IMultiUpdater : IUpdater
{
	void AddSubscriber(IUpdatable subscriber, int indexOverride);	
}

public interface ISingleUpdater : IUpdater
{
	void RemoveSubscriber();
}