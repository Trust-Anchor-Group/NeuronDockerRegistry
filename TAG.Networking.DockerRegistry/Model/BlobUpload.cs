using System;

namespace TAG.Networking.DockerRegistry.Model
{
	/// <summary>
	/// Contains information about a current upload.
	/// </summary>
	public class BlobUpload : IDisposable
	{
		/// <summary>
		/// Contains information about a current upload.
		/// </summary>
		/// <param name="Uuid">UUID of upload.</param>
		public BlobUpload(Guid Uuid)
		{
			this.Uuid = Uuid;
		}

		/// <summary>
		/// UUID of upload.
		/// </summary>
		public Guid Uuid { get; }

		/// <summary>
		/// Disposes of the upload.
		/// </summary>
		public void Dispose()
		{
		}
	}
}
