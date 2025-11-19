using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TAG.Networking.DockerRegistry.Model;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;

namespace TAG.Networking.DockerRegistry
{
    [CollectionName("DockerRepository")]
    [TypeName(TypeNameSerialization.None)]
    [Index("RepositoryName")]
    [Index("OwnerGuid")]
    [Index("OwnerType")]
    [Index("IsPrivate")]
    public class DockerRepository
    {
        private string objectId;
        private string repositoryName;
        private Guid ownerGuid;
        private DockerActorType ownerType;
        private bool isPrivate;

        /// <summary>
        /// Object ID
        /// </summary>
        [ObjectId]
        public string ObjectId
        {
            get => this.objectId;
            set => this.objectId = value;
        }

        /// <summary>
        /// Name of the repository.
        /// </summary>
        public string RepositoryName
        {
            get => this.repositoryName;
            set => this.repositoryName = value;
        }

        /// <summary>
        /// Is the repository private?
        /// </summary>
        public bool IsPrivate
        {
            get => this.isPrivate;
            set => this.isPrivate = value;
        }

        /// <summary>
        /// Repository owner Guid
        /// </summary>
        public Guid OwnerGuid
        {
            get => this.ownerGuid;
            set => this.ownerGuid = value;
        }

        /// <summary>
        /// Repository owner Guid
        /// </summary>
        public DockerActorType OwnerType
        {
            get => this.ownerType;
            set => this.ownerType = value;
        }

        public DockerRepository()
        {

        }

        public DockerRepository(string RepositoryName, IDockerActor Actor)
        {
            this.repositoryName = RepositoryName;
            this.ownerGuid = Actor.GetGuid();
            this.ownerType = Actor.GetActorType();
        }

        public bool HasPermission(IDockerActor Actor, RepositoryAction Action)
        {
            switch (Action)
            {
                case RepositoryAction.Pull:
                    if (!IsPrivate)
                        return true;
                    break;
                case RepositoryAction.Delete:
                case RepositoryAction.Push:
                    if (Actor.GetGuid() == OwnerGuid && Actor.GetActorType() == OwnerType)
                        return true;
                    break;
            }

            return false;
        }

        public async Task<IDockerActor> GetOwner()
        {
            switch (OwnerType)
            {
                case DockerActorType.User:
                    return await Database.FindFirstDeleteRest<DockerUser>(new FilterAnd(new FilterFieldEqualTo("Guid", OwnerGuid)));
                case DockerActorType.Organization:
                    return await Database.FindFirstDeleteRest<DockerOrganization>(new FilterAnd(new FilterFieldEqualTo("Guid", OwnerGuid)));
                default:
                    throw new Exception("Invalid actor type.");
            }
        }

        public static bool ValidateRepositoryName(string name)
        {
            Regex ComponentRegex = new Regex("^[a-z0-9]+(?:[._-][a-z0-9]+)*$", RegexOptions.Compiled);

            if (string.IsNullOrEmpty(name))
                return false;

            if (name.Length > 255)
                return false;

            string[] components = name.Split('/');
            foreach (var component in components)
            {
                if (string.IsNullOrEmpty(component))
                    return false;

                if (!ComponentRegex.IsMatch(component))
                    return false;
            }

            return true;
        }

        public enum RepositoryAction
        {
            Pull,
            Push,
            Delete,
        }
    }
}
