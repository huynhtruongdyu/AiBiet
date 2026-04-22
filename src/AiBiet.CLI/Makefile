.DEFAULT_GOAL := pack

CONFIG := Release
PACKAGE_ID := AiBiet.CLI
NUGET_CONFIG := ../../NuGet.Config

pack:
	dotnet pack . -c $(CONFIG)

install: pack
	dotnet tool install --global $(PACKAGE_ID) --configfile $(NUGET_CONFIG)

update: pack
	dotnet tool update --global $(PACKAGE_ID) --configfile $(NUGET_CONFIG)

uninstall:
	dotnet tool uninstall --global $(PACKAGE_ID)

reinstall: uninstall install

run:
	dotnet run -- $(ARGS)

clean:
	dotnet clean .
# 	rm -f ./bin/Release/*.nupkg