using System;
using Waher.Persistence;

namespace TAG.Networking.DockerRegistry
{
    [Serializable]
    public class ActorOption
    {
        public CaseInsensitiveString Name;
        public object Value;

        public ActorOption()
        {

        }

        public ActorOption(CaseInsensitiveString Name, object Value)
        {
            this.Name = Name;
            this.Value = Value;
        }
    }
}
