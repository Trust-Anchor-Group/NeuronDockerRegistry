Title: Registry Sniffer
Master: /Master.md
JavaScript: /Events.js
JavaScript: StorageSniffer.js
CSS: /DockerRegistry/Style.cssx
UserVariable: User
Privilege: DockerRegistry.Read
Login: /Login.md
Parameter: Guid

{{
    Storage := select top 1 * from TAG.Networking.DockerRegistry.Model.DockerStorage where Guid=Guid;

    if Storage = null then
        NotFound("No storage with guid " + Guid);

	DockerDashboardAssertPermisions(Storage, "DockerRegistry.Read");
    "";
}}

================================================

# Storage Sniffer: {{Guid}}

================================================

<div id="sniffer-view">'