
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
    public class DockerOrganization : DockerActorAuthentification
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
        public Guid ActorGuid { get; set; }

        /// <summary>
        /// The username of the broker account
        /// </summary>
		public CaseInsensitiveString OrganizationName { get; set; }
        public override Guid GetActorGuid()
        {
            return ActorGuid;
        }
    }
}