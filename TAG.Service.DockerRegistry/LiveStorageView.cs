using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TAG.Networking.DockerRegistry.Model;
using Waher.Content;
using Waher.IoTGateway;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Cache;

namespace TAG.Service.DockerRegistry
{
    public class LiveStorageView : HttpSynchronousResource, IHttpPostMethod, IDisposable
    {
        private static readonly TimeSpan timeout = TimeSpan.FromMinutes(15);
        private static readonly Cache<Guid, Watcher> watchers = new Cache<Guid, Watcher>(int.MaxValue, timeout, timeout, true);

        public LiveStorageView(string ResourceName) : base(ResourceName)
        {
            Database.ObjectUpdated += async (object Sender, ObjectEventArgs args) =>
            {
                if (watchers.Values.Count < 1)
                    return;
                if (args.Object is DockerStorage Storage)
                {
                    string[] Tabs = watchers.Values.Where((Watcher) => { return Watcher.StorageGuid == Storage.Guid; }).Select(Watcher => Watcher.TabId).ToArray();

                    await ClientEvents.PushEvent(Tabs, "StorageUpdated", Storage);
                }
            };
        }

        public bool AllowsPOST => true;

        public override bool HandlesSubPaths => false;

        public override bool UserSessions => true;

        public async Task POST(HttpRequest Request, HttpResponse Response)
        {
            ContentResponse Content = await Request.DecodeDataAsync();

            if (Content.HasError || !(Content.Decoded is Dictionary<string, object> Json))
            {
                await Response.SendResponse(new UnsupportedMediaTypeException("Expected JSON."));
                return;
            }

            if (!Json.TryGetValue("tab", out object TabObj) || !(TabObj is string TabID) || string.IsNullOrEmpty(TabID))
            {
                await Response.SendResponse(new BadRequestException("Missing Tab ID."));
                return;
            }

            if (!Json.TryGetValue("storageGuid", out object StorageGuidObj) || !(StorageGuidObj is string StorageGuidStr) || !Guid.TryParse(StorageGuidStr, out Guid StorageGuid))
            {
                await Response.SendResponse(new BadRequestException("Invalid or missing storage guid."));
                return;
            }

            watchers.Add(Guid.NewGuid(), new Watcher()
            {
                StorageGuid = StorageGuid,
                TabId = TabID,
            });

            DockerStorage Storage = await Database.FindFirstIgnoreRest<DockerStorage>(new FilterAnd(new FilterFieldEqualTo("Guid", StorageGuid)));

            await Response.Return(Storage);
        }

        public void Dispose()
        {
            watchers.Clear();
        }

        private class Watcher
        {
            public string TabId;
            public Guid StorageGuid;
        }
    }
}
