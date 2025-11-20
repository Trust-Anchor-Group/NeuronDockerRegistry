function StorageUpdated(storage)
{
    const div = document.getElementById("sniffer-view");
    const children = storage.BlobCounter.map(x =>
    {
        const p = document.createElement("p");
        p.innerText = `${x.ReferenceCount} - ${x.Digest.HashFunction}:${x.Digest.Hash}`;
        return p;
    });
    div.replaceChildren(...children);
}

window.addEventListener("DOMContentLoaded", async () =>
{
    const urlParams = new URLSearchParams(window.location.search);
    const storageGuid = urlParams.get("Guid") || "";

    const res = await fetch("/DockerRegistry/live-storage-view", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Accept": "application/json"
        },
        body: JSON.stringify({
            tab: TabID,
            storageGuid: storageGuid
        })
    });

    const storage = await res.json();
    StorageUpdated(storage);
});