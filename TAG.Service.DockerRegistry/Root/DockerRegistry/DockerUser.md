Title: Users
Copyright: /Copyright.md
Master: /Master.md
JavaScript: /Events.js
JavaScript: Docker.js
JavaScript: /TargetBlank.js
Script: /Controls/SimpleTable.script
UserVariable: User
Privilege: Admin.Docker
Login: /Login.md
CSS: Style.cssx
Parameter: Guid

{{
DockerUser := select top 1 * from TAG.Networking.DockerRegistry.Model.DockerUser where Guid=Guid;

if DockerUser = null then
	NotFound("User with guid " + Guid + " does not exist.");

Storage := DockerUser.GetStorage();

if exists(Posted) then
(
	if Posted matches { "delete": Bool(PDelete) } and PDelete = true then (
		DeleteObject(DockerUser);
		TemporaryRedirect("DockerUsers.md");
	);

	if Posted matches { "maxStorage": Number(PMaxStorage) } and PMaxStorage > 0 then (
		Storage.MaxStorage:= PMaxStorage;
		UpdateObject(Storage);
	);

		if Posted matches { "brokerAccount": String(PBrokerAccount) } then (
		DockerUser.AccountName:=PBrokerAccount;
		UpdateObject(DockerUser);
	);
);
"";
}}

================================================================================================================================

# Docker User: {{DockerUser.AccountName}}

============================================================================

<div class="docker-double">
	<form method="POST" onsubmit="DockerAreYouSure(event, 'Are you sure you want to update the storage limit?')">
		<h2>Update Storage</h2>
		<input name="maxStorage" value="{{Storage.MaxStorage}}">
		<button>Update</button>
	</form>
	<form method="POST" onsubmit="DockerAreYouSure(event, 'Are you sure you want to update the owner account?')">
		<h2>Broker Account</h2>
		<input name="brokerAccount" value="{{DockerUser.AccountName}}">
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

<div class="docker-double">

{{
PrepareTable(()->
(
	Page.Order:="RepositoryName";
	TAG.Networking.DockerRegistry.Model.IDockerActor.FindOwnedImages(DockerUser)
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

{{
PrepareTable(()->
(
	Page.Order:="Repositories";
	select * from DockerRepository where OwnerGuid = Guid
));

}}

| {{Header("Repository","RepositoryName")}} | 
|:----------|
{{foreach Repository in Page.Table do
(
	]]| ((Repository.RepositoryName)) [[;
	]]|
[[
)
}}

</div>


============================================================================

<div class="docker-row">
	<form method="POST" onsubmit="DockerAreYouSure(event, 'Are you sure you want to delete this user and all its repositories?')">
		<input name="delete" value="true" hidden>
		<button class="negButton">Delete Docker User</button>
	</form>
	<button onclick="OpenPage('DockerStorage.md?Guid={{Storage.Guid}}')">Storage</button>
	<div>
		<p><small>Actor GUID: {{DockerUser.Guid}}</small></p>
		<p><small>Sstorage GUID: {{Storage.Guid}}</small></p>
	</div>
</div>
