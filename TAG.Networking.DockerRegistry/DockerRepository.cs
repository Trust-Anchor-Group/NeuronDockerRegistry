using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TAG.Networking.DockerRegistry.Model;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
using Waher.Service.IoTBroker.StateMachines.Model.Actions.Runtime;

namespace TAG.Networking.DockerRegistry
{
    [CollectionName("DockerRepository")]
    [TypeName(TypeNameSerialization.None)]
    [Index("RepositoryName")]
    [Index("OwnerGuid")]
    [Index("IsPrivate")]
    public class DockerRepository
    {
        private static readonly Regex RootSegmentPattern = new Regex(@"^[A-Za-z0-9._-]+$", RegexOptions.Compiled);

        private string objectId;
        private string repositoryName;
        private Guid ownerGuid;
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

        public DockerRepository()
        {

        }

        public DockerRepository(string RepositoryName, DockerActor Actor)
        {
            this.repositoryName = RepositoryName;
            this.ownerGuid = Actor.Guid;
        }

        public bool HasPermission(DockerActor Actor, RepositoryAction Action)
        {
            if (Actor == null)
                return false;

            switch (Action)
            {
                case RepositoryAction.Pull:
                    if (!IsPrivate)
                        return true;
                    break;
                case RepositoryAction.Delete:
                case RepositoryAction.Push:
                    if (Actor.Guid == OwnerGuid)
                        return true;
                    break;
            }

            return false;
        }

        public async Task<DockerActor> GetOwner()
        {
            return await Database.FindFirstDeleteRest<DockerActor>(new FilterAnd(new FilterFieldEqualTo("Guid", OwnerGuid)));
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

        public static bool IsValidRootName(string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            if (!Name.EndsWith('/'))
                return false;

            var segment = Name.Substring(0, Name.Length - 1); // drop trailing '/'

            if (segment.Length == 0)
                return false;

            if (segment.Contains("/"))
                return false;

            if (!RootSegmentPattern.IsMatch(segment))
                return false;

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
