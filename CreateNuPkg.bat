@echo off

.\.nuget\nuget.exe pack .\StackExchange.Redis.DataTypes\StackExchange.Redis.DataTypes.csproj -Prop Configuration=Release -Build -OutputDirectory .\.nuget\