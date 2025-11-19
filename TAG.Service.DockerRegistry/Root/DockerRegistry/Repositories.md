Title: Repositories
Copyright: /Copyright.md
Master: /Master.md
JavaScript: /Events.js
Script: /Controls/SimpleTable.script
CSS: Style.cssx
UserVariable: User
Privilege: Admin.Docker
Login: /Login.md

================================================================================================================================

Docker Repositories
===================


## Create repository

{{
	if (exists(Posted) and Posted matches { "RepositoryName": PRepositoryName, "OwnerGuid": POwnerGuid, "Visibility": PVisibility }) then
	(
		DockerCreateRepository(PRepositoryName, POwnerGuid, PVisibility = "private");
		]]+> created repository at ((PRepositoryName)) [[
	);
}}

<form action="" method="POST">
	<p>
		<label for="RepositoryName">Repository Name</label>  
		<input type="text" id="RepositoryName" name="RepositoryName" autofocus required/>
	</p>
	<p>
		<label for="OwnerGuid">Owner GUID</label>  
		<input type="text" id="OwnerGuid" name="OwnerGuid" autofocus required/>
	</p>

	<p>
		<label for="Visibility">Visibility</label>  
		<select name="Visibility" id="Visibility" required>
		<option value="public" selected>Public</option>
		<option value="private">Private</option>
		</select>
	</p>

	<button>Create user</button>
</form>


## Repositories



{{
PrepareTable(()->
(
	Page.Order:="RepositoryName";
	select * from TAG.Networking.DockerRegistry.DockerRepository order by RepositoryName
));
}}

| {{Header("Name","DockerRepositoryName")}} | {{Header("Owner Type","OwnerType")}} |
|:----------|:-------:|
{{foreach Repository in Page.Table do
(
	]]| [((MarkdownEncode(UN:=Repository.RepositoryName);))] [[;
	]]| [((MarkdownEncode(EM:=Repository.OwnerType);))] [[;
    ]]|
[[
)
}}