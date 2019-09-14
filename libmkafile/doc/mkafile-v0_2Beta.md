# mka binary data container - Version 0.2Beta

## Introduce mka binary data container
mka binary data container is data container format for application which published by mkaraki apps.

## File header structure

### Static Header
|Position|Headers|Type|Bytes|
|:--|:--|:--|--:|
|0|Binary Byte (`0x07`)|Binary|1|
|1|mkafile header (`mkafile`)|ASCII string|7|
|8|mkafile version (`0x02`)|binary|2|

Binary Byte (at position 0) is `0x07`.

mkafile header (at position 1) is string like `mkafile`

mkafile version (at position 8) is binary value. (**Fill by Internal Managed Id**) (Version 0.2Beta is `0x02`)

### Dynamic Header
|Position|Headers|Type|Bytes|
|:--|:--|:--|--:|
|9|Application Name Length|unsigned 8bit - integer|1|
|10|Application Name|ASCII string|dynamic (**ref:** Application Name Length)|
|Dynamic (Next)|Container Description Length|unsigned 16bit - integer|2|
|+2|Container Description|ASCII string|dynamic (**ref:** Container Description Length)|

Dynamic header will contain the application name and container information (e.x. `mkafile tester`: test mkafile for check mkafile parser)

Application name length range is **1 to 255**.

Container Description length range is **1 to 65,025**.

If application don't want to write descrption, fill with `NO INFORMATION`.

## File structure

**Remember:** It only can save **less than 2,147,483,647 bytes** of value data and value name string data.
**Remember:** Length Property cannot use 0 or less value.
**Remember:** Length Property saves **used bytes** even if name property.

### Each objects structure

- normal object case:

|Headers|Type|Bytes|
|:--|:--|--:|
|value name length|signed 32bit - integer|4|
|value name|UTF-8 string|dynamic (**ref:** value name length)|
|value type|Binary|1|
|value length|signed 32bit - integer|4|
|value|dynamic (**ref:** value type)|dynamic (**ref:** value length)|

- null object case:

|Headers|Type|Bytes|
|:--|:--|--:|
|value name length|signed 32bit - integer|4|
|value name|UTF-8 string|dynamic (**ref:** value name length)|
|value type (`0x00`)|Binary|1|
|actual value type|Binary|1|

- static length object case (e.x. boolean, 32bit int):

|Headers|Type|Bytes|
|:--|:--|--:|
|value name length|signed 32bit - integer|4|
|value name|UTF-8 string|dynamic (**ref:** value name length)|
|value type|Binary|1|
|value|dynamic (**ref:** value type)|object length|

### Value Type List
|Hex code|Type|Length|
|:--|:--|--:|
|0x00|null object|0|
|0x01|binary|-|
|0x02|boolean|1|
|0x03|32bit integer|4|
|0x04|32bit unsigned integer|4|
|0x05|64bit integer|8|
|0x06|64bit unsigned integer|8|
|0x07|UTF-8 string|-|