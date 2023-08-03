using Waher.Persistence.Attributes;
using Waher.Security;

namespace TAG.Networking.DockerRegistry.Model
{
	/// <summary>
	/// A Docker BLOB reference
	/// </summary>
	[CollectionName("DockerBlobs")]
	[TypeName(TypeNameSerialization.None)]
	[Index("Digest", "Function")]
	[Index("AccountName", "Digest", "Function")]
	public class DockerBlob
	{
		/// <summary>
		/// A Docker BLOB reference
		/// </summary>
		public DockerBlob()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// Hash function used
		/// </summary>
		public HashFunction Function { get; set; }

		/// <summary>
		/// Digest
		/// </summary>
		public byte[] Digest { get; set; }

		/// <summary>
		/// File name
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Name of user account uploading the BLOB.
		/// </summary>
		public string AccountName { get; set; }
	}
}
