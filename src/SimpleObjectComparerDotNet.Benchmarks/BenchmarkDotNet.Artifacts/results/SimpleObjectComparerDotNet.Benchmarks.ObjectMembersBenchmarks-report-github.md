``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22621.608)
12th Gen Intel Core i9-12900F, 1 CPU, 24 logical and 16 physical cores
.NET SDK=7.0.100-rc.1.22431.12
  [Host]     : .NET 6.0.9 (6.0.922.41905), X64 RyuJIT AVX2
  Job-RYANJR : .NET 6.0.9 (6.0.922.41905), X64 RyuJIT AVX2

Runtime=.NET 6.0  Toolchain=net6.0  

```
|                Method |     Mean |     Error |    StdDev |
|---------------------- |---------:|----------:|----------:|
|  ObjectMembers_simple | 1.489 μs | 0.0085 μs | 0.0071 μs |
| ObjectMembers_complex | 7.222 μs | 0.0431 μs | 0.0404 μs |
