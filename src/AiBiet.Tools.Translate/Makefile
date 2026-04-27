.DEFAULT_GOAL := pack

CONFIG := Release
TFM := net10.0

OUT := bin/$(CONFIG)/$(TFM)
PACKAGES_DIR := ../../packages
PLUGINS_DIR := $(PACKAGES_DIR)/plugins

pack:
	dotnet pack . -c $(CONFIG) -o $(PACKAGES_DIR)
# 	cp $(OUT)/*.dll $(PLUGINS_DIR)/ || true
# 	cp $(OUT)/*.deps.json $(PLUGINS_DIR)/ || true
# 	cp $(OUT)/*.pdb $(PLUGINS_DIR)/ || true

clean:
	dotnet clean .
	rm -f $(PACKAGES_DIR)/*.nupkg
# 	rm -f $(PLUGINS_DIR)/*.dll
# 	rm -f $(PLUGINS_DIR)/*.deps.json
# 	rm -f $(PLUGINS_DIR)/*.pdb

push-github:
	dotnet nuget push $(PACKAGES_DIR)/AiBiet.Tools.Translate.0.1.0.nupkg --source github --api-key [GITHUB_PAT]