# FiniteLivesHelper
## Purpose
FiniteLivesHelper is a helper mod for Celeste intended to give the player a finite number of lives in your maps. Originally, players have infinite lives to get across different screens.

## Formatting
FiniteLivesHelper uses the XML file format to parse the number of lives that a player would have when entering a level. This should be stored as `finitelives.xml` in the root of your map archive.
Below is an example of a correctly-formatted file.
```xml
<?xml version="1.0" encoding="utf-8" ?>
<finitelives>
	<chapter name="1-ForsakenCity">
		<level name="a-00" lives="1"></level>
		<level name="a-01" lives="0"></level>
		<level name="a-02" lives="not_a_number"></level>
		<level name="a-03" lives="-892"></level>
	</chapter>
	<chapter name="iamdadbod/0/IamdadbodCollab">
		<level name="a-00" lives="9999"></level>
	</chapter>
</finitelives>
```
Chapter names should be the file path without the `Maps` directory path and the `.bin` extension, and the level name should be whatever you named it.
Non-positive numbers and non-numbers represent infinite lives.

## Usage
Build `finite-lives-helper.sln` and insert the built `finite-lives-helper.dll` into your mod archive. Of course, edit your `everest.yaml` accordingly. Tested and working with Everest build 1992.

## TODO
- User-setting that adjusts y-position of life display
- Fade left/right animation on player spawn for life display
- Submit the mod to gamebanana.com
- Ability to disable "Save and Quit" menu option
