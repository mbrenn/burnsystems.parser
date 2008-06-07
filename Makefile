MCS_FLAGS = -warn:4 -langversion:LINQ
MCS = gmcs

CSHARP_FILES = *.cs \
	Dcss/*.cs \
	Helper/*.cs \
	Properties/*.cs

BURNSYSTEMSPARSER_REFERENCES = ../bin/Release/BurnSystems.dll,System.Web

bin/Release/BurnSystems.Parser.dll: $(CSHARP_FILES)
	mkdir -p bin
	mkdir -p bin/Release
	$(MCS) $(MCS_FLAGS) -out:bin/Release/BurnSystems.Parser.dll -r:$(BURNSYSTEMSPARSER_REFERENCES) -target:library $(CSHARP_FILES) 

.PHONY: clean
clean:
	rm -rf bin
