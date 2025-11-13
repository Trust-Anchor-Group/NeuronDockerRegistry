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
Storage := DockerUser.GetStorage();

if (exists(Posted) and Posted matches { "delete": Bool(PDelete) } and PDelete = true) then (
	DeleteObject(DockerUser);
	TemporaryRedirect("DockerUsers.md");
);

"";
}}

================================================================================================================================

DockerUser: {{DockerUser.AccountName}}
===================


============================================================================

## Delete User
<form method="POST">
	<input name="delete" value="true" hidden>
	<button>Delete</button>
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