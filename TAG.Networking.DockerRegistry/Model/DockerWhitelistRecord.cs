using System;
using Waher.Persistence.Attributes;

namespace TAG.Networking.DockerRegistry.Model
{
    [CollectionName("DockerWhitelistRecord")]
    [TypeName(TypeNameSerialization.None)]
    [Index("PermittingActor", "PermittedActor")]
    [Index("PermittedActor", "PermittingActor")]
    public class DockerWhitelistRecord
    {
        /// <summary>
        /// Object ID
        /// </summary>
        [ObjectId]
        public string ObjectId { get; set; }

        /// <summary>
        /// The actor giving privillages
        /// </summary>
        public Guid PermittingActor { get; set; }

        /// <summary>
        /// The actor reciving privillages
        /// </summary>
        public Guid PermittedActor { get; set; }
    }
}
