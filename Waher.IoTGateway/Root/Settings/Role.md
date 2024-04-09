﻿Title: {{RoleId}}
Description: Allows editing of a role.
Date: 2021-01-27
Author: Peter Waher
Master: /Master.md
JavaScript: /Controls/SimpleTree.js
JavaScript: /Settings/Next.js
JavaScript: /Events.js
CSS: /Controls/SimpleTree.cssx
CSS: /Settings/Config.cssx
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Security.Roles
Login: /Login.md
Parameter: RoleId
Parameter: Add

========================================================================

{{
Role:=select top 1 * from Waher.Security.Users.Role where Id=RoleId;
if !exists(Role) then 
(
	if Add then
		Role:=Create(Waher.Security.Users.Role)
	else
		NotFound("Role not found: "+RoleId)
);

if exists(Posted) then
(
	Privileges:=Create(System.Collections.Generic.List,Waher.Security.Users.PrivilegePattern);
	if !empty(Posted.Privileges) then
	(
		foreach Privilege in Posted.Privileges.Split(Waher.Content.CommonTypes.CRLF,System.StringSplitOptions.RemoveEmptyEntries) do
		(
			if Privilege.StartsWith("+") then
				Privileges.Add(Create(Waher.Security.Users.PrivilegePattern, Privilege.Substring(1), true))
			else if Privilege.StartsWith("-") then
				Privileges.Add(Create(Waher.Security.Users.PrivilegePattern, Privilege.Substring(1), false))
			else
				Privileges.Add(Create(Waher.Security.Users.PrivilegePattern, Privilege, true));
		)
	);

	if exists(Posted.Id) then
	(
		if empty(Posted.Id) then BadRequest("ID must not be empty.");
		Role.Id:=Posted.Id;
	);
	
	Role.Description:=Posted.Description ??? "";
	Role.Privileges:=Privileges.ToArray();

	if empty(Role.ObjectId) then
	(
		SaveNewObject(Role);
		LogInformation("Role created.",{"Object":Role.Id,"Actor":User.UserName});
	)
	else
	(
		UpdateObject(Role);
		LogInformation("Role updated.",{"Object":Role.Id,"Actor":User.UserName});
	);

	Waher.Security.Users.Roles.ClearCache();
	ReloadPage("/Settings/Roles.md");
	
	if Add then SeeOther("Role.md?RoleId="+Role.Id);
);

empty(Role.Id) ? "Add new role" : Role.Id
}}
===================

<form action="Role.md" method="post" enctype="multipart/form-data">
<fieldset>
<legend>Role definition</legend>

{{if Add then ]]
<p>
<label for="Id">ID:</label>  
<input type="text" id="Id" name="Id" value='((Role.Id))' autofocus required/>
</p>
[[}}

<p>
<label for="Description">Description:</label>  
<textarea id="Description" name="Description">{{Role.Description}}</textarea>
</p>

<p>
<label for="Privileges">Privileges:</label>  
<textarea id="Privileges" name="Privileges">{{
if exists(Role.Privileges) then
(
	foreach Privilege in Role.Privileges do
	(
		if Privilege.Include then ]]+[[ else ]]-[[;
		]]((Privilege.Expression))
[[
	)
)}}</textarea>
<small>Privileges are defined as a set of [regular expressions](https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference), 
each row an expression. The first character in each row defines if the expression adds a privilege (`+`) or removes a privilege (`-`). When determining if a role has a
privilege, the list is processed from top to bottom.</small>
</p>

<button type="submit" class="posButton">{{Add?"Add":"Apply"}}</button>
<button type="button" class="negButton" onclick="Reload('')">Cancel</button>

<small>**Note**: Any changes made to a role will be available to users once they log out, and back in again.</small>

</fieldset>
</form>

Registered Privileges
=======================

Privileges are registered dynamically, as services check for them. They are organized in a tree-structure, with root privileges and
sub-privileges (or child privileges), delimited by period (`.`) characters. You can browse the privileges requested so far, below.

<ul class="SimpleTree">
{{
RootPrivileges:=select * from Waher.Security.Users.Privilege where ParentFullId="";
foreach Privilege in RootPrivileges do
	]]<li class="Expandable" onclick="ExpandNode(event,this)" data-id="((Privilege.FullId))" data-expand="ExpandPrivilege.ws" 
	data-collapsedimg="((HtmlAttributeEncode(MarkdownToHtml(":file_folder:") ) ))" 
	data-expandedimg="((HtmlAttributeEncode(MarkdownToHtml(":open_file_folder:") ) ))"><span class="ItemImage">:file_folder:</span> `((Privilege.LocalId))`</li>
[[
}}
</ul>