@echo off
Set DOMAIN=%1
Set USERNAME=%2
Set PASSWORD=%3

@echo "Updating Connection.cs from %DOMAIN%"
.\tools\CrmSvcUtil.exe /url:https://%DOMAIN%/XRMServices/2011/Organization.svc?wsdl /o:Connection.pre /n:Contoso.Extensions.Connections.Crm /serviceContextName:CrmServiceContext /u:%USERNAME% /p:%PASSWORD%
powershell -Command "(Get-Content Connection.pre -Raw).Replace('public string EntityLogicalName','public string EntityLogicalName_') | Set-Content Connection.cs"
del Connection.pre