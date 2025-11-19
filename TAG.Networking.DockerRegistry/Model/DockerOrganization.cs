
using System;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;

namespace TAG.Networking.DockerRegistry.Model
{
    [CollectionName("DockerOrganization")]
    [Index("Guid")]
    [Index("OrganizationName")]
    public class DockerOrganization : IDockerActor
    {
        /// <summary>
        /// A Docker User
        /// </summary>
        public DockerOrganization()
        {

        }

        /// <summary>
        /// Object ID
        /// </summary>
        [ObjectId]
        public string ObjectId { get; set; }

        /// <summary>
        /// Actor guid
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The username of the broker account
        /// </summary>
		public CaseInsensitiveString OrganizationName { get; set; }

        /// <summary>
        /// Docker storage guid
        /// </summary>
        public Guid Storage;

        public async Task<DockerStorage> GetStorage()
        {
            return await Database.FindFirstIgnoreRest<DockerStorage>(new FilterAnd(new FilterFieldEqualTo("Guid", Storage)));
        }

        public bool HasPermision(DockerRepository Repository, DockerRepository.RepositoryAction Action)
        {
            return false;
        }

        public Guid GetGuid()
        {
            return Guid;
        }

        public DockerActorType GetActorType()
        {
            return DockerActorType.Organization;
        }
    }
}