.DEFAULT_GOAL := pack

CONFIG := Release
PACKAGE_ID := AiBiet.CLI
PACKAGE_PATH := ./bin/$(CONFIG)

pack:
	dotnet pack . -c $(CONFIG)

install: pack
	dotnet tool install --global --add-source $(PACKAGE_PATH) $(PACKAGE_ID)

update: pack
	dotnet tool update --global --add-source $(PACKAGE_PATH) $(PACKAGE_ID)

uninstall:
	dotnet tool uninstall --global $(PACKAGE_ID)

reinstall: uninstall pack
	dotnet tool install --global --add-source $(PACKAGE_PATH) $(PACKAGE_ID)

run:
	dotnet run -- $(ARGS)

clean:
	dotnet clean .
# 	rm -f ./bin/Release/*.nupkg