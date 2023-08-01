using System.Text.RegularExpressions;
using Waher.Networking.HTTP;

namespace TAG.Networking.DockerRegistry
{
	/// <summary>
	/// Docker Registry API v2.
	/// 
	/// Reference:
	/// https://docs.docker.com/registry/spec/api/
	/// </summary>
	public class RegistryServerV2 : HttpSynchronousResource
	{
		private  static readonly Regex regexName = new Regex("[a-z0-9]+(?:[._-][a-z0-9]+)*", RegexOptions.Compiled | RegexOptions.Singleline);

		/// <summary>
		/// Docker Registry API v2.
		/// </summary>
		public RegistryServerV2()
			: base("/v2")
		{
		}

		/// <summary>
		/// If resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => true;

		/// <summary>
		/// If resource uses sessions (i.e. uses a session cookie).
		/// </summary>
		public override bool UserSessions => true;

		/// <summary>
		/// Checks if a Name is a valid Docker name.
		/// </summary>
		/// <param name="Name">Name</param>
		/// <returns>If <paramref name="Name"/> is a valid Docker name.</returns>
		public bool IsName(string Name)
		{
			Match M = regexName.Match(Name);
			return M.Success && M.Index == 0 && M.Length == Name.Length;
		}
	}
}
