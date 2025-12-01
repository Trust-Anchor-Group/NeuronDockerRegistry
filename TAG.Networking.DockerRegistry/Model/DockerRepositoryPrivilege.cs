using System;
using Waher.Persistence.Attributes;

namespace TAG.Networking.DockerRegistry.Model
{
    [CollectionName("DockerRepositoryPrivilege")]
    [TypeName(TypeNameSerialization.None)]
    [Index("RepositoryGuid", "ActorGuid")]
    [Index("ActorGuid", "RepositoryGuid")]

    public class DockerRepositoryPrivilege
    {
        /// <summary>
        /// Object ID
        /// </summary>
        [ObjectId]
        public string ObjectId { get; set; }

        public Guid RepositoryGuid { get; set; }

        /// <summary>
        /// The actor reciving privillages
        /// </summary>
        public Guid ActorGuid { get; set; }

        /// <summary>
        /// If the actor can do write operation on the repository
        /// </summary>
        public bool AllowWrite { get; set; }

        /// <summary>
        /// If the actor can do read operation on the repository
        /// </summary>
        public bool AllowRead { get; set; }
    }
}
