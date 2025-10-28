# GrandInt (Meow.Math.Number)

## 简介

本仓库实现了任意精度大整数类型 `GrandInt`（属于 `Meow.Math.Number`），适用于需要高精度整数运算、定制解析规则与紧凑序列化的场景。

##主要特性

- 内部表示：小端字节数组 `byte[] _magnitude`（索引0 为最低有效字节），使用 `int Length` 表示有效字节数；`byte Flags` 存放符号等标志（最低位为符号位）。
- 序列化：格式为 `[8 字节 little-endian 的 ulong length][Flags][数据字节...]`。协议层将 `length`视作无符号，运行时受 `int.MaxValue` 限制。
-解析：支持十进制、十六进制（`0x...`）和二进制（`0b...`），允许使用下划线作为分隔符。提供 `Parse` / `TryParse`。
- 算法实现：按字节实现加、减、乘（schoolbook／逐字节算法），关键内部循环使用 `unsafe` + `fixed` 优化性能。
- 内存与性能：后备数组按倍数增长以减少重分配；解析路径尽量减少临时分配；提供 `GetHeapAllocatedSize()` 报告数组容量，`GetStackAllocatedSize()` 报告逻辑栈使用。

## 项目结构

仓库按项目划分（位于 `Meow.Math.Number`目录）：

- `BigNumber/` — 核心库，包含 `GrandInt` 实现及工具函数。
- `BigNumberExample/` — 示例程序，展示构造、序列化、解析与运算用法。
- `BigNumberTest/` — 单元测试（若存在），用于验证运算与序列化的正确性。
- `Benchmarks/` — 基准测试项目（可选，依赖 BenchmarkDotNet）。

##设计与实现要点

-低级操作以字节为单位实现，便于控制进位/借位并复用数组。
- 为了提升性能，部分路径使用 `unsafe`，项目启用了 `AllowUnsafeBlocks`。
- 若需支持超过 `int.MaxValue` 的逻辑长度，应考虑基于块（block）或分片的存储设计。

## 快速开始（示例）

```csharp
var x = GrandInt.Parse("12345678901234567890");
Console.WriteLine(x.Length);
var data = x.ToSerialized();
var y = GrandInt.FromSerialized(data);
```

## 构建

- 使用 .NET SDK（仓库含针对 .NET8 与 .NET Standard2.0 的项目）。
- 在仓库根目录运行：
 - `dotnet build` 构建所有项目
 - `dotnet test`运行测试（若 `BigNumberTest` 存在）
 - `dotnet run --project BigNumberExample`运行示例

## 注意事项

- 项目启用了 `unsafe`以优化关键路径；若环境不允许 `unsafe`，需要重写相关实现为受限的安全版本。
- 序列化格式保留8 字节长度字段以保证协议兼容，但运行时长度受 `int.MaxValue` 限制。
- 可视需要引入 `ArrayPool<byte>` 或其他池化方案以进一步降低垃圾回收开销。

##贡献

欢迎提交 issue 或 PR。对核心算法或序列化格式的修改请先讨论兼容性。

##许可证

请查看仓库根目录的 `LICENSE` 文件以获取许可信息。

----

本文件为仅中文说明，便于中文读者快速了解项目用途、结构与使用方式。

