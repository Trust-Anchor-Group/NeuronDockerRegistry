using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;
namespace TAG.Networking.DockerRegistry.Model
{
    [CollectionName("DockerStorage")]
    [Index("Guid")]
    public class DockerStorage
    {
        /// <summary>
        /// Object ID
        /// </summary>
        [ObjectId]
        public string ObjectId { get; set; }

        /// <summary>
        /// Actor Guid
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Blob reference counters
        /// </summary>
        public DigestReferenceCounter[] BlobCounter { get; set; }

        /// <summary>
        /// Max amount unique blob data in bytes
        /// </summary>
        public long MaxStorage { get; set; }

        /// <summary>
        /// Amount of unique blob data referenced in bytes
        /// </summary>
        public long UsedStorage { get; set; }

        /// <summary>
        /// A blob reference counter and storage tracker
        /// </summary>
        public DockerStorage()
        {

        }

        public DockerStorage(long MaxStorage)
        {
            this.MaxStorage = MaxStorage;
        }

        public async Task RegistrerImage(IImageManifest Image)
        {
            await IncrementDigest(Image.GetConfig().Digest);

            foreach (IImageLayer Layer in Image.GetLayers())
            {
                await IncrementDigest(Layer.Digest);
            }
        }

        public async Task UnregisterImage(IImageManifest Image)
        {
            await DecrementDigest(Image.GetConfig().Digest);

            foreach (IImageLayer Layer in Image.GetLayers())
            {
                await DecrementDigest(Layer.Digest);
            }
        }

        private async Task IncrementDigest(HashDigest Digest)
        {
            int index = Array.BinarySearch(BlobCounter, new DigestReferenceCounter() { Digest = Digest });

            if (index < 0)
            {
                // new blob
                DockerBlob Blob = await Database.FindFirstIgnoreRest<DockerBlob>(new FilterAnd(new FilterFieldEqualTo("Digest", Digest)));

                if (Blob == null)
                {
                    Log.Critical("Tried to increment with blob which does not exist");
                    return;
                }

                UsedStorage += Blob.Size;

                List<DigestReferenceCounter> BlobCounterList = new List<DigestReferenceCounter>(this.BlobCounter ?? Array.Empty<DigestReferenceCounter>());
                BlobCounterList.Add(new DigestReferenceCounter()
                {
                    Digest = Digest,
                    ReferenceCount = 1
                });

                BlobCounterList.Sort();

                BlobCounter = BlobCounterList.ToArray();
            }
            else
            {
                BlobCounter[index].ReferenceCount++;
            }
        }

        private async Task DecrementDigest(HashDigest Digest)
        {
            int index = Array.BinarySearch(BlobCounter, new DigestReferenceCounter() { Digest = Digest });

            if (index < 0)
            {
                Log.Critical("Tried to decrement digest which was not registed");
            }
            else
            {
                BlobCounter[index].ReferenceCount--;

                if (BlobCounter[index].ReferenceCount < 1)
                {
                    List<DigestReferenceCounter> BlobCounterList = new List<DigestReferenceCounter>(this.BlobCounter ?? Array.Empty<DigestReferenceCounter>());
                    BlobCounterList.RemoveAt(index);
                    DockerBlob Blob = await Database.FindFirstIgnoreRest<DockerBlob>(new FilterAnd(new FilterFieldEqualTo("Digest", Digest)));

                    if (Blob == null)
                    {
                        Log.Critical("Tried to decrement with blob which does not exist");
                        return;
                    }

                    BlobCounter = BlobCounterList.ToArray();

                    UsedStorage -= Blob.Size;
                }
            }
        }

        public static string UIString(DockerStorage Storage)
        {
            return ToMetricBytes(Storage.UsedStorage) + " of " + ToMetricBytes(Storage.MaxStorage);
        }

        private static string ToMetricBytes(double value)
        {
            string[] units = { "", "K", "M", "G", "T", "P" };
            int unit = 0;

            while (value >= 1000 && unit < units.Length - 1)
            {
                value /= 1000;
                unit++;
            }

            // format: no decimals for ints, otherwise max 2 dp
            string formatted = value % 1 == 0
                ? value.ToString("0")
                : value.ToString("0.##");

            return $"{formatted}{units[unit]}b";
        }
    }

    [Serializable]
    public class DigestReferenceCounter : IComparable
    {
        public HashDigest Digest;
        public int ReferenceCount;

        public int CompareTo(object obj)
        {
            if (obj is DigestReferenceCounter Other)
                return Digest.CompareTo(Other.Digest);
            else
                throw new ArgumentException("Object is not a DigestReferenceCounter.", nameof(obj));
        }
    }
}
