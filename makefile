
CS_FILES = $(shell find src/ -type f -name *.cs)
TESTS_CS_FILES = $(shell find tests/ -type f -name *.cs)

all: bin/BurnSystems.Parser.dll bin/BurnSystems.Parser.UnitTests.dll

bin/BurnSystems.Parser.dll: $(CS_FILES)
	xbuild src/BurnSystems.Parser.csproj
	mkdir -p bin
	cp src/bin/Debug/* bin/

bin/BurnSystems.Parser.UnitTests.dll: $(TESTS_CS_FILES)
	xbuild tests/BurnSystems.Parser.UnitTests/BurnSystems.Parser.UnitTests.csproj
	mkdir -p bin
	cp tests/BurnSystems.Parser.UnitTests/bin/Debug/* bin/

.PHONY: install
install: all
	mkdir -p ~/lib/mono
	cp bin/* ~/lib/mono

.PHONY: test
test: all
	nunit-console -labels bin/BurnSystems.Parser.UnitTests.dll

.PHONY: clean
clean:
	rm -fR src/bin
	rm -fR src/obj
	rm -fR bin
