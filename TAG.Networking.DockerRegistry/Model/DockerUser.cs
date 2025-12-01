using System;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace TAG.Networking.DockerRegistry.Model
{
    [TypeName(TypeNameSerialization.FullName)]
    [Index("AccountName")]
    public class DockerUser : DockerActor
    {
        /// <summary>
        /// A Docker User
        /// </summary>
        public DockerUser()
        {

        }

        /// <summary>
        /// The username of the broker account
        /// </summary>
		public CaseInsensitiveString AccountName { get; set; }
    }
}
