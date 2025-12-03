Title: Docker Registry
Author: Peter Waher
Copyright: /Copyright.md
Master: /Master.md
JavaScript: /Events.js
Script: /Controls/SimpleTable.script
JavaScript: /TargetBlank.js
JavaScript: /Sniffers/Sniffer.js
UserVariable: User
Privilege: DockerRegistry
CSS: Style.cssx
Login: /Login.md
Parameter: Purge

{{
	if exists(Posted) then (
		Authorize(User, "Administrator.DockerRegistry)
		if Posted matches { "forceClean": Bool(PForceClean) } then (
			]] <p>Blobs cleaned: ((TAG.Service.DockerRegistry.RegistryService.Instance.CleanUnusedBlobs();)) </p>[[;
			]] <p>((TAG.Service.DockerRegistry.RegistryService.Instance.CleanUnmanagedRepositories();)) </p>[[;
		);
	);
	"";
}}


================================================================================================================================

Docker Registry Settings
===================

## Dashboards

<button onclick="OpenPage('DockerUsers.md')">Users</button>
<button onclick="OpenPage('DockerOrganizations.md')">Organizations</button>
<button onclick="OpenPage('Repositories.md')">Repositories</button>

## Data

<button class="posButton"{{
if User.HasPrivilege("Admin.Communication.DockerRegistry") and User.HasPrivilege("Admin.Communication.Sniffer") then
	" onclick=\"OpenSniffer('Sniffers/Sniffer.md')\""
else
	" disabled"
}}>Sniffer</button>

{{
	if (User.HasPrivilage("Administrator.DockerRegistry")) then (
		]]
		<form method="POST">
			<input name="forceClean" value="true" hidden/>
			<button>Force Clean</button>
		</form>
		[[
	)
}}