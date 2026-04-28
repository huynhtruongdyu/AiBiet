.DEFAULT_GOAL := pack

CONFIG := Release
TFM := net10.0
VERSION := 0.1.0

OUT := bin/$(CONFIG)/$(TFM)
PACKAGES_DIR := ../../packages
PLUGINS_DIR := $(PACKAGES_DIR)/plugins

pack:
	dotnet pack . -c $(CONFIG) -o $(PACKAGES_DIR)

clean:
	dotnet clean .
	rm -f $(PACKAGES_DIR)/*.nupkg

push-github:
	dotnet nuget push $(PACKAGES_DIR)/AiBiet.Tools.Commit.$(VERSION).nupkg --source github --api-key [GITHUB_PAT]

push-nuget-test:
	dotnet nuget push $(PACKAGES_DIR)/AiBiet.Tools.Commit.$(VERSION).nupkg --source nuget-test --api-key [NUGETTEST_PAT]

push: push-github push-nuget-test