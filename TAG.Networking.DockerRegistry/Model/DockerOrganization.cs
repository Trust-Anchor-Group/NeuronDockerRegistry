
using System;
using Waher.Persistence;
using Waher.Persistence.Attributes;
namespace TAG.Networking.DockerRegistry.Model
{
    [TypeName(TypeNameSerialization.FullName)]
    [Index("OrganizationName")]
    public class DockerOrganization : DockerActor
    {
        /// <summary>
        /// A Docker User
        /// </summary>
        public DockerOrganization()
        {
        }

        /// <summary>
        /// The username of the broker account
        /// </summary>
		public CaseInsensitiveString OrganizationName { get; set; }
    }
}