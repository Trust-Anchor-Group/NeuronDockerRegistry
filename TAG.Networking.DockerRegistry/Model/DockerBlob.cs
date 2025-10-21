using Waher.Persistence.Attributes;

namespace TAG.Networking.DockerRegistry.Model
{
	/// <summary>
	/// A Docker BLOB reference
	/// </summary>
	[CollectionName("DockerBlobs")]
	[TypeName(TypeNameSerialization.None)]
	[Index("Digest")]
	[Index("AccountName", "Digest")]
	[Index("Image", "Digest")]
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
		/// Digest
		/// </summary>
		public HashDigest Digest { get; set; }

		/// <summary>
		/// File name
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Name of user account uploading the BLOB.
		/// </summary>
		public string AccountName { get; set; }

		/// <summary>
		/// Name of image to which the BLOB corresponds.
		/// </summary>
		public string Image { get; set; }
	}
}
