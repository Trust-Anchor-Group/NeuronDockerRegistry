using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;

namespace TAG.Networking.DockerRegistry.Model
{
    [CollectionName("DockerUser")]
    [Index("Guid")]
    [Index("UserName")]
    public class DockerUser : IDockerActor
    {
        /// <summary>
        /// A Docker User
        /// </summary>
        public DockerUser()
        {

        }

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
        /// The username of the broker account
        /// </summary>
		public CaseInsensitiveString AccountName { get; set; }

        /// <summary>
        /// Docker storage guid
        /// </summary>
        public Guid Storage { get; set; }

        public async Task<DockerStorage> GetStorage()
        {
            return await Database.FindFirstIgnoreRest<DockerStorage>(new FilterAnd(new FilterFieldEqualTo("Guid", Storage)));
        }

        public Guid GetGuid()
        {
            return Guid;
        }

        public DockerActorType GetActorType()
        {
            return DockerActorType.User;
        }
    }
}
