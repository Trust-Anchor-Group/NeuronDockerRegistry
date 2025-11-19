Title: Users
Copyright: /Copyright.md
Master: /Master.md
JavaScript: /Events.js
Script: /Controls/SimpleTable.script
UserVariable: User
Privilege: Admin.Docker
Login: /Login.md
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
);
"";
}}

================================================================================================================================

DockerUser: {{DockerUser.AccountName}}
===================

{{DockerUser.Guid}}

============================================================================

## Delete User
<form method="POST">
	<input name="delete" value="true" hidden>
	<button>Delete</button>
</form>

## Update Storage
<form method="POST">
	<input name="maxStorage" value="{{Storage.MaxStorage}}">
	<button>Update</button>
</form>

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