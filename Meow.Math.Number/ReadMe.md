# GrandInt (Meow.Math.Number)

Summary of current implementation and usage notes.

- Internal storage: `byte[] _magnitude` little-endian (LSB at index0), `int Length` for used bytes, `byte Flags` (LSB sign bit).
- Serialization: `[8-byte little-endian ulong length][Flags][data bytes...]`. Length is protocol-level unsigned; runtime limited to `int.MaxValue`.
- Parsing: `Parse`/`TryParse` support decimal, hex (`0x..`) and binary (`0b..`), underscores allowed as separators.
- Arithmetic: implemented at byte-level (add/sub/mul schoolbook). Multiplication and addition use `unsafe` + `fixed` internal loops for speed.
- Memory/perf:
 - Internal array grows by doubling; ensure capacity keeps reallocations down.
 - Parsing uses in-place array helpers to reduce temporary allocations; further pooling can be added.
 - `GetHeapAllocatedSize()` reports backing array length; `GetStackAllocatedSize()` reports logical stack usage (1 + IntPtr.Size).

Notes:
- The project enables `AllowUnsafeBlocks` for internal optimizations. Remove if unsafe is undesired.
- If you need support for magnitudes > `int.MaxValue`, consider block-based storage.

Examples

```csharp
var x = GrandInt.Parse("12345678901234567890");
Console.WriteLine(x.Length);
var data = x.ToSerialized();
var y = GrandInt.FromSerialized(data);
