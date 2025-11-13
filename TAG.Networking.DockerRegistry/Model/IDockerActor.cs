using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;

namespace TAG.Networking.DockerRegistry.Model
{
    public interface IDockerActor
    {
        public Guid GetGuid();
        public DockerActorType GetActorType();
        public Task<DockerStorage> GetStorage();
        public static async Task<DockerImage[]> FindOwnedImages(IDockerActor Actor)
        {
            DockerRepository[] Repositories = (await Database.Find<DockerRepository>(new FilterAnd(
                new FilterFieldEqualTo("OwnerGuid", Actor.GetGuid()),
                new FilterFieldEqualTo("OwnerType", Actor.GetActorType())))).ToArray();

            List<DockerImage> DockerImages = new List<DockerImage>();
            foreach (DockerRepository Repository in Repositories)
            {
                DockerImage[] Images = (await Database.Find<DockerImage>(new FilterFieldEqualTo("RepositoryName", Repository.RepositoryName))).ToArray();
                DockerImages.AddRange(Images);
            }

            return DockerImages.ToArray();
        }

        public static async Task ReSyncStorage(IDockerActor Actor)
        {
            DockerStorage Storage = await Actor.GetStorage();
            if (Storage is null)
                return;

            DockerImage[] Images = await FindOwnedImages(Actor);

            Storage.UsedStorage = 0;
            Storage.BlobCounter = new DigestReferenceCounter[0];

            foreach (DockerImage Image in Images)
            {
                await Storage.RegistrerImage(Image.Manifest);
            }

            await Database.Update(Storage);
        }
    }

    public enum DockerActorType
    {
        None,
        User,
        Organization,
    }
}
