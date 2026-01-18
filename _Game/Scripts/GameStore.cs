using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using SPACE_UTIL;

namespace SPACE_GAME
{
	[DefaultExecutionOrder(-50)] // just after new InputSystem init
	public class GameStore : MonoBehaviour
	{
		public static InputActionAsset IA;
		public static PlayerStats playerStats;
		[SerializeField] InputActionAsset _IA;

		private void Awake()
		{
			Debug.Log(C.method(this));
			GameStore.IA = this._IA;
			GameStore.LoadInit();
		}
		static void LoadInit()
		{
			GameStore.IA.tryLoadBindingOverridesFromJson(LOG.LoadGameData(GameDataType.inputKeyRebindings));
			GameStore.playerStats = LOG.LoadGameData<PlayerStats>(GameDataType.playerStats);
		}

		#region sample load/save gameData(playerStats)
		float currTime = 0f;
		private void Update()
		{
			this.currTime += Time.unscaledDeltaTime;
		}
		private void OnApplicationQuit()
		{
			Debug.Log(C.method(this, "orange"));
			//
			string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
			GameStore.playerStats.HISTORY.Add($"{sceneName}: {this.currTime}");
			GameStore.playerStats.totalGameTime += this.currTime;
			GameStore.playerStats.Save();
		}

		[System.Serializable]
		public class PlayerStats
		{
			public List<string> HISTORY = new List<string>();
			public float totalGameTime = 0f;
			// Public API >>
			
			public void Save()
			{
				LOG.SaveGameData(GameDataType.playerStats, GameStore.playerStats.ToJson());
			}
			// << Public API
		} 
		#endregion
	}

	// GLOBAL ENUM >>
	public enum GameDataType
	{
		inputKeyRebindings,
		playerStats,
	}
	// << GLOBAL ENUM
}