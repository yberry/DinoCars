using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FXManager : MonoBehaviour {

	public bool autoDestroy;
	protected bool played;
	public List<ParticleSystem> particleEffects = new List<ParticleSystem>();
	// Use this for initialization
	void Start () {
	
	}

	protected virtual void Update()
	{
		if (autoDestroy && played) {
			bool allFinished = true;
			foreach (var ps in particleEffects) {
				allFinished &= ( ps.isStopped && !ps.IsAlive(true));
			}
			if (allFinished) {
				Destroy(gameObject, 1f);
			}
		}
	}
	
	// Update is called once per frame
	public void PlayOnce (bool withChildren=true) {
		played = true;

		foreach (var ps in particleEffects)
		{
			if (ps.isPlaying)
				ps.Stop();
			var main = ps.main;
			main.loop = false;
			ps.Play(withChildren);
		}


	}

	public void PlayLoop(bool withChildren = true)
	{
		played = true;

		foreach (var ps in particleEffects) {
			if (ps.isPlaying)
				ps.Stop();
			var main = ps.main;
			main.loop = true;
			ps.Play(withChildren);
	
		}
	}

	public void Stop(bool withChildren = true)
	{

		foreach (var ps in particleEffects)
		{
			if (ps.isPlaying)
				ps.Stop(withChildren);

		}
	}

	public void Pause(bool withChildren = true)
	{

		foreach (var ps in particleEffects)
		{
			if (ps.isPlaying)
				ps.Pause(withChildren);

		}
	}
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(FXManager))]
class DecalMeshHelperEditor : UnityEditor.Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		if ( Application.isPlaying && GUILayout.Button("Play all"))
			(target as FXManager).PlayOnce();
	}
}
#endif