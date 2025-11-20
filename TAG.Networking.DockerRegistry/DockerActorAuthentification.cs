using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TAG.Networking.DockerRegistry.Model;
using Waher.Persistence;
using Waher.Persistence.Filters;

namespace TAG.Networking.DockerRegistry
{
    public abstract class DockerActorAuthentification
    {
        public abstract Guid GetActorGuid();
        public async Task<DockerActor> GetActor()
        {
            return await Database.FindFirstIgnoreRest<DockerActor>(new FilterAnd(new FilterFieldEqualTo("Guid", GetActorGuid())));
        }
        public async Task<DockerStorage> GetStorage()
        {
            return await (await GetActor()).GetStorage();
        }
    }
}
