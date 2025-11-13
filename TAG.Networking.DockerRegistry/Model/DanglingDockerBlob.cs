using System;
using Waher.Persistence.Attributes;

namespace TAG.Networking.DockerRegistry.Model
{
    [CollectionName("DanglingDockerBlob")]
    public class DanglingDockerBlob
    {
        /// <summary>
        /// An object representing unused docker blobs
        /// </summary>
        public DanglingDockerBlob()
        {

        }

        /// <summary>
        /// Object ID
        /// </summary>
        [ObjectId]
        public string ObjectId { get; set; }

        /// <summary>
        /// Digest of the dangling blob
        /// </summary>
        public HashDigest Digest { get; set; }

        /// <summary>
        /// Creation timestamp.
        /// </summary>
        public DateTime Created { get; set; }
    }
}
