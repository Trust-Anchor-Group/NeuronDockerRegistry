using Waher.Persistence.Attributes;

namespace TAG.Networking.DockerRegistry.Model
{
	/// <summary>
	/// A Docker Image reference
	/// </summary>
	[CollectionName("DockerImages")]
	[TypeName(TypeNameSerialization.None)]
	[Index("AccountName", "Image", "Tag")]
	[Index("Image", "Tag")]
	[Index("Image", "Digest")]
	public class DockerImage
	{
		/// <summary>
		/// A Docker Image reference
		/// </summary>
		public DockerImage()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// Name of user account uploading the Image.
		/// </summary>
		public string AccountName { get; set; }

		/// <summary>
		/// Name of image.
		/// </summary>
		public string Image { get; set; }

		/// <summary>
		/// Image Tag.
		/// </summary>
		public string Tag { get; set; }

		/// <summary>
		/// Image Digest
		/// </summary>
		public HashDigest Digest { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public IImageManifest Manifest { get; set; }
    }
}
