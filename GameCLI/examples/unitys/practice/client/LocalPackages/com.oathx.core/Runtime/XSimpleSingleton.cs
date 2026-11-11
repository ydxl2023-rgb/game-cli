
namespace Oathx.Core
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class XSimpleSingleton<T> where T : class, new()
	{
		/// <summary>
		/// The instance.
		/// </summary>
		protected static readonly T instance = new T();

		/// <summary>
		/// Gets the singleton.
		/// </summary>
		/// <returns>The singleton.</returns>
		public static T GetSingleton()
		{
			return instance;
		}
	}
}
