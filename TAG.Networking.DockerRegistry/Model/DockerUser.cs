using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Script.Operators.Arithmetics;

namespace TAG.Networking.DockerRegistry.Model
{
    [CollectionName("DockerUser")]
    [Index("Guid")]
    [Index("UserName")]
    public class DockerUser : DockerActorAuthentification
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
        public Guid ActorGuid { get; set; }

        /// <summary>
        /// The username of the broker account
        /// </summary>
		public CaseInsensitiveString AccountName { get; set; }

        public override Guid GetActorGuid()
        {
            return ActorGuid;
        }
    }
}
