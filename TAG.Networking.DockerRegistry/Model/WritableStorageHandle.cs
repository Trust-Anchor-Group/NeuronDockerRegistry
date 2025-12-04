using System;
using System.Threading.Tasks;
using Waher.Persistence;

namespace TAG.Networking.DockerRegistry.Model
{
    public class WritableStorageHandle : IAsyncDisposable
    {
        private Waher.Runtime.Threading.Semaphore semaphore;
        public DockerStorage Storage { get; }
        public WritableStorageHandle(DockerStorage Storage, Waher.Runtime.Threading.Semaphore Semaphore)
        {
            this.Storage = Storage;
            this.semaphore = Semaphore;
        }
        public async ValueTask DisposeAsync()
        {
            await Database.Update(Storage);
            await semaphore?.DisposeAsync();
            semaphore = null;
        }
    }
}
