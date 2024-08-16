using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Security;

namespace TAG.Networking.DockerRegistry.Model
{
	/// <summary>
	/// Contains information about a current upload.
	/// </summary>
	public class BlobUpload : IDisposable
	{
		private SemaphoreSlim synchObj = new SemaphoreSlim(1);

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
		/// BLOB reference object.
		/// </summary>
		public DockerBlob Blob { get; set; }

		/// <summary>
		/// Reference to file stream object.
		/// </summary>
		public FileStream File { get; set; }

		/// <summary>
		/// Name of upload file.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Locks the upload record.
		/// </summary>
		public async Task Lock()
		{
			if (this.synchObj is null)
				throw new ObjectDisposedException("Upload has been terminated and disposed.");

			await this.synchObj.WaitAsync();
		}

		/// <summary>
		/// Releases the upload record.
		/// </summary>
		public void Release()
		{
			if (this.synchObj is null)
				throw new ObjectDisposedException("Upload has been terminated and disposed.");

			this.synchObj.Release();
		}

		/// <summary>
		/// Computes the Hash Digest, given a Hash function.
		/// </summary>
		/// <param name="Function">Hash Function</param>
		/// <returns>BLOB Digest</returns>
		public async Task<byte[]> ComputeDigest(HashFunction Function)
		{
			await this.Lock();
			try
			{
				return this.ComputeDigestLocked(Function);
			}
			finally
			{
				this.Release();
			}
		}

		/// <summary>
		/// Computes the Hash Digest, given a Hash function. Assumes the record has been locked by the caller.
		/// </summary>
		/// <param name="Function">Hash Function</param>
		/// <returns>BLOB Digest</returns>
		internal byte[] ComputeDigestLocked(HashFunction Function)
		{
			if (this.File is null)
				return new byte[0];
			else
			{
				this.File.Position = 0;
				return Hashes.ComputeHash(Function, this.File);
			}
		}

		/// <summary>
		/// Disposes of the upload.
		/// </summary>
		public void Dispose()
		{
			try
			{
				this.synchObj?.Dispose();
				this.synchObj = null;

				this.File?.Dispose();
				this.File = null;

                if (System.IO.File.Exists(this.FileName))
					System.IO.File.Delete(this.FileName);
            }
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}
	}
}
