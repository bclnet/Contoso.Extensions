@echo off
@echo "Update wsdl file from Setup->Develop(build)->API->Generate Enterprise WSDL then Generate"
@echo "Updating Connection.cs from wsdl file"
.\tools\SvcUtil.exe wsdl.xml /tcv:Version35 /o:Connection.cs /n:urn:fault.enterprise.soap.sforce.com,Contoso.Extensions.Connections.Sfdc.Fault /n:*,Contoso.Extensions.Connections.Sfdc /noconfig
powershell -Command "(Get-Content Connection.cs -Raw).Replace('[][]','[]') | Set-Content Connection.cs"
