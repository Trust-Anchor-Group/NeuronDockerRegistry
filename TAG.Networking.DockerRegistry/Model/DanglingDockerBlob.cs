using System;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;

namespace TAG.Networking.DockerRegistry.Model
{
    [CollectionName("DanglingDockerBlob")]
    [TypeName(TypeNameSerialization.None)]
    [Index("Owner")]
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

        /// <summary>
        /// Final size of the blob upload
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Guid of the owner, responsible for the upload
        /// </summary>
        public Guid Owner { get; set; }

        public async Task OnDanglingBlobDeleted()
        {
            DockerActor Actor = await Database.FindFirstIgnoreRest<DockerActor>(new FilterAnd(new FilterFieldEqualTo("Guid", Owner)));
            if (Actor != null)
            {
                await using WritableStorageHandle Handle = await Actor.GetWritableStorage();
                await Handle.Storage.UnregisterDanglingBlob(this);
            }
        }
    }
}
