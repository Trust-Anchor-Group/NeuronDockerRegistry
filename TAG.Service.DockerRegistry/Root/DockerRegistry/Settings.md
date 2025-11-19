Title: Docker Registry
Author: Peter Waher
Copyright: /Copyright.md
Master: /Master.md
JavaScript: /Events.js
Script: /Controls/SimpleTable.script
JavaScript: /TargetBlank.js
JavaScript: /Sniffers/Sniffer.js
UserVariable: User
CSS: Style.cssx
Privilege: Admin.Docker
Login: /Login.md
Parameter: Purge

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
	" onclick=\"OpenSniffer('Sniffer.md')\""
else
	" disabled"
}}>Sniffer</button>