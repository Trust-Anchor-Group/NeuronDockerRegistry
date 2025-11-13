using System;
using Waher.Persistence.Attributes;

namespace TAG.Networking.DockerRegistry.Model
{
    /// <summary>
    /// A Docker BLOB reference
    /// </summary>
    [CollectionName("DockerBlobs")]
    [TypeName(TypeNameSerialization.None)]
    [Index("Digest")]
    public class DockerBlob : IComparable
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
        /// File path
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Size of blob file in bytes
        /// </summary>
        public long Size { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is DockerBlob Other)
                return Digest.CompareTo(Other.Digest);
            else
                throw new ArgumentException("Object is not a DigestReferenceCounter.", nameof(obj));
        }
    }
}
