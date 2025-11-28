
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Runtime.Threading;

namespace TAG.Networking.DockerRegistry.Model
{
    [CollectionName("DockerActor")]
    [TypeName(TypeNameSerialization.FullName)]
    public abstract class DockerActor
    {
        /// <summary>
        /// Object ID
        /// </summary>
        [ObjectId]
        public string ObjectId { get; set; }

        /// <summary>
        /// Actor Guid
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Docker storage guid
        /// </summary>
        public Guid StorageGuid { get; set; }

        /// <summary>
        /// Docker storage guid
        /// </summary>
        public ActorOptions Options;

        public DockerActor()
        {
            this.Options = new ActorOptions();
        }

        public async Task<WritableStorageHandle> GetWritableStorage()
        {
            Waher.Runtime.Threading.Semaphore Semaphore = await Semaphores.BeginWrite("DockerRegistry_StorageAffecting_" + Guid);
            DockerStorage Storage = await Database.FindFirstIgnoreRest<DockerStorage>(new FilterAnd(new FilterFieldEqualTo("Guid", StorageGuid)));
            return new WritableStorageHandle(Storage, Semaphore);
        }

        public async Task<ReadOnlyStorageHandle> GetReadOnlyStorage()
        {
            Waher.Runtime.Threading.Semaphore Semaphore = await Semaphores.BeginRead("DockerRegistry_StorageAffecting_" + Guid);
            DockerStorage Storage = await Database.FindFirstIgnoreRest<DockerStorage>(new FilterAnd(new FilterFieldEqualTo("Guid", StorageGuid)));
            return new ReadOnlyStorageHandle(Storage, Semaphore);
        }

        public async Task<DockerImage[]> FindOwnedImages()
        {
            DockerRepository[] Repositories = (await Database.Find<DockerRepository>(new FilterAnd(new FilterFieldEqualTo("OwnerGuid", Guid)))).ToArray();

            List<DockerImage> DockerImages = new List<DockerImage>();
            foreach (DockerRepository Repository in Repositories)
            {
                DockerImage[] Images = (await Database.Find<DockerImage>(new FilterFieldEqualTo("RepositoryName", Repository.RepositoryName))).ToArray();
                DockerImages.AddRange(Images);
            }

            return DockerImages.ToArray();
        }

        public async Task ReSyncStorage()
        {
            await using WritableStorageHandle StorageHandle = await GetWritableStorage();
            if (StorageHandle is null)
                return;

            DockerImage[] Images = await FindOwnedImages();

            StorageHandle.Storage.UsedStorage = 0;
            StorageHandle.Storage.BlobCounter = new DigestReferenceCounter[0];

            foreach (DockerImage Image in Images)
            {
                await StorageHandle.Storage.RegistrerImage(Image.Manifest);
            }
        }
    }
}
