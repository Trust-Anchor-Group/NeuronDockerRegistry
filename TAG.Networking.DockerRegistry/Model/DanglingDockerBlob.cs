using System;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Runtime.Threading;
using static TAG.Networking.DockerRegistry.Model.DockerActor;

namespace TAG.Networking.DockerRegistry.Model
{
    [CollectionName("DanglingDockerBlob")]
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

        public async Task UnregistreFromStorage()
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
