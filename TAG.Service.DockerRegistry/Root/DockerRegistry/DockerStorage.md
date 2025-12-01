Title: Registry Sniffer
Master: /Master.md
JavaScript: /Events.js
JavaScript: /TargetBlank.js
UserVariable: User
Privilege: Admin.Communication.Sniffer
Privilege: Admin.Communication.DockerRegistry
Login: /Login.md
Parameter: Guid

{{
    Storage := select top 1 * from DockerStorage where Guid=Guid;

    if Storage = null then (
        NotFound("No storage with guid " + Guid);
    );


    if Exists(Posted) then (
        if Posted matches { "resync": Bool(PResync )} then (
            Actor:=select top 1 * from DockerActor where StorageGuid=Guid;

            if Actor = null then
                NotFound("There is no owner of this storage");

            Actor.ReSyncStorage();
        );
    );
}}


=============================================

# Storage: {{Guid}}
<h2>
StorageUsed: {{
	Used := ToMetricBytes(Storage.UsedStorage);
    Max := ToMetricBytes(Storage.MaxStorage);
	]] ((Used)) / ((Max)) [[;
}}
</h2>

<form method="POST">
    <input name="resync" value="true" hidden/>
    <button>Resync counters</button>
</form>
<button class="posButton" onclick="OpenPage('/DockerRegistry/Sniffers/StorageSniffer.md?Guid={{Guid}}')">Sniffer</button>