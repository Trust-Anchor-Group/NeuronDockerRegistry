using System;
using System.Threading.Tasks;
using Waher.Content.Html.Elements;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Runtime.Threading;

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
            IDockerActor Actor = await Database.FindFirstIgnoreRest<IDockerActor>(new FilterAnd(new FilterFieldEqualTo("Guid", Owner)));
            if (Actor != null)
            {
                using Semaphore Semaphore = await IDockerActor.StorageSemaphore(Actor);
                DockerStorage Storage = await Actor.GetStorage();
                await Storage.UnregisterDanglingBlob(this);
            }
        }
    }
}
