using System;
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
        /// Name of image.
        /// </summary>
        public string RepositoryName { get; set; }

        /// <summary>
        /// Image Tag.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Manifest Digest
        /// </summary>
        public HashDigest Digest { get; set; }

        /// <summary>
        /// Manifest Digest
        /// </summary>
        public IImageManifest Manifest { get; set; }

        public long GetSize()
        {
            long Size = 0;

            foreach (IImageLayer Layer in Manifest.GetLayers())
            {
                Size += Layer.Size;
            }

            return Size;
        }
    }
}
