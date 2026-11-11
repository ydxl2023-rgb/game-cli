using UnityEngine;

namespace Oathx.Core
{
	/// <summary>
	/// Mono behaviour singleton.
	/// </summary>
	public class XMonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		/// <summary>
		/// The instance.
		/// </summary>
		public static T instance;

		/// <summary>
		/// Gets the singleton.
		/// </summary>
		/// <returns>The singleton.</returns>
		public static T GetSingleton()
		{
			if (!instance)
			{
				GameObject singleton = new GameObject(typeof(T).Name);
				if (!singleton)
					throw new System.NullReferenceException();

				instance = singleton.AddComponent<T>();
				GameObject.DontDestroyOnLoad(singleton);
			}

			return instance;
		}
	}
}


