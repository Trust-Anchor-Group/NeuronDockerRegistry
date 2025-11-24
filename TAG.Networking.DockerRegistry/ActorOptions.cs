using System;
using Waher.Persistence;

namespace TAG.Networking.DockerRegistry
{
    public class ActorOptions
    {
        public static readonly CaseInsensitiveString CanAutoCreateRepository = "CanAutoCreateRepository";
        public static readonly CaseInsensitiveString AutoCreateRepositoryRoot = "AutoCreateRepositoryRoot";

        private ActorOption[] options;

        public ActorOption[] Options
        {
            get { return options; }
            set { options = value; }
        }

        public ActorOptions()
        {
            options = new ActorOption[0];
        }

        public void SetOption(CaseInsensitiveString Name, object Value)
        {
            for (int i = 0; i < this.options.Length; i++)
            {
                if (this.options[i].Name == Name)
                {
                    this.options[i].Value = Value;
                    return;
                }
            }

            int Length = options.Length;
            Array.Resize(ref this.options, Length + 1);
            options[Length] = new ActorOption(Name, Value);
        }

        public bool IsOptionTrue(CaseInsensitiveString Name)
        {
            foreach (ActorOption Option in options)
            {
                if (Option.Name == Name && Option.Value is bool Value && Value == true)
                    return true;
            }
            return false;
        }

        public bool TryGetOption(CaseInsensitiveString Name, out object Value)
        {
            foreach (ActorOption Option in options)
            {
                if (Option.Name == Name)
                {
                    Value = Option.Value;
                    return true;
                }
            }

            Value = null;
            return false;
        }

        public object TryGetOptionWithDefault(CaseInsensitiveString Name, object Default)
        {
            if (TryGetOption(Name, out object Value))
                return Value;
            return Default;
        }
    }
}
