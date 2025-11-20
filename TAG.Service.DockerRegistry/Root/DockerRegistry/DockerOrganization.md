Title: Docker Organization
Copyright: /Copyright.md
Master: /Master.md
JavaScript: /Events.js
JavaScript: Docker.js
Script: /Controls/SimpleTable.script
UserVariable: User
Privilege: Admin.Docker
Login: /Login.md
CSS: Style.cssx
Parameter: Guid

{{
DockerOrganization := select top 1 * from TAG.Networking.DockerRegistry.Model.DockerOrganization where ObjectId=Guid;

if DockerOrganization = null then
	NotFound("Organization with guid " + Guid + " does not exist.");

Actor := DockerOrganization.GetActor();
Storage := Actor.GetStorage();
if exists(Posted) then
(
	if Posted matches { "delete": Bool(PDelete) } and PDelete = true then (
		DeleteObject(DockerOrganization);
		TemporaryRedirect("DockerOrganizations.md");
	);

	if Posted matches { "maxStorage": Number(PMaxStorage) } and PMaxStorage > 0 then (
		Storage.MaxStorage:= PMaxStorage;
		UpdateObject(Storage);
	);

		if Posted matches { "organizationName": String(POrganizationName) } then (
		DockerOrganization.OrganizationName:=POrganizationName;
		UpdateObject(DockerOrganization);
	);
);
"";
}}

================================================================================================================================

# Docker Organization: {{DockerOrganization.OrganizationName}}

============================================================================

<div class="docker-double">
	<form method="POST" onsubmit="DockerAreYouSure(event, 'Are you sure you want to update the storage limit?')">
		<h2>Update Storage</h2>
		<input name="maxStorage" value="{{Storage.MaxStorage}}">
		<button>Update</button>
	</form>
	<form method="POST" onsubmit="DockerAreYouSure(event, 'Are you sure you want to update the organization name?')">
		<h2>Broker Account</h2>
		<input name="organizationName" value="{{DockerOrganization.OrganizationName}}">
		<button>Update</button>
	</form>
<div>



============================================================================

<h2>
StorageUsed: {{
	Used := ToMetricBytes(Storage.UsedStorage);
    Max := ToMetricBytes(Storage.MaxStorage);
	]] ((Used)) / ((Max)) [[;
}}
</h2>

<br>
{{
PrepareTable(()->
(
	Page.Order:="RepositoryName";
	Actor.FindOwnedImages();
));

}}

| {{Header("Repository","RepositoryName")}}  | {{Header("Tag", "Tag")}} | {{Header("Size", "Size")}} | {{Header("Digest", "Digest")}} | 
|:----------|:--------|:----------|:----------|
{{foreach Image in Page.Table do
(
	Size:=ToMetricBytes(Image.GetSize());
	]]| ((Image.RepositoryName)) [[;
	]]| ((Image.Tag)) [[;
	]]| ((Size)) [[;
	]]| ((Image.Digest)) [[;
	]]|
[[
)
}}



============================================================================

<div class="docker-row">
	<form method="POST" onsubmit="DockerAreYouSure(event, 'Are you sure you want to delete this organization and all its repositories?')">
		<input name="delete" value="true" hidden>
		<button class="negButton">Delete Docker Organization</button>
	</form>
	<div>
		<p><small>Actor GUID: {{DockerOrganization.ActorGuid}}</small></p>
		<p><small>Sstorage GUID: {{Storage.Guid}}</small></p>
	</div>
</div>
