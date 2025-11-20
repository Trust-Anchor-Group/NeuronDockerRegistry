using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Runtime.Threading;

namespace TAG.Networking.DockerRegistry.Model
{
    public class DockerActor
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
        public Guid Storage { get; set; }

        public async Task<DockerStorage> GetStorage()
        {
            return await Database.FindFirstIgnoreRest<DockerStorage>(new FilterAnd(new FilterFieldEqualTo("Guid", Storage)));
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
            using Semaphore Semaphore = await StorageSemaphore();
            DockerStorage Storage = await GetStorage();
            if (Storage is null)
                return;

            DockerImage[] Images = await FindOwnedImages();

            Storage.UsedStorage = 0;
            Storage.BlobCounter = new DigestReferenceCounter[0];

            foreach (DockerImage Image in Images)
            {
                await Storage.RegistrerImage(Image.Manifest);
            }

            await Database.Update(Storage);
        }

        public async Task<Semaphore> StorageSemaphore()
        {
            return await Semaphores.BeginWrite("DockerRegistry_StorageAffecting_" + Guid);
        }
    }
}
