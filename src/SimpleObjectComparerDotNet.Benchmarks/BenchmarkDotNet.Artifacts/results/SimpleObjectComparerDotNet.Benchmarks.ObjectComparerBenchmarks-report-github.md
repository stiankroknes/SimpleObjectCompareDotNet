``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22000.1042/21H2)
12th Gen Intel Core i9-12900F, 1 CPU, 24 logical and 16 physical cores
.NET SDK=7.0.100-rc.1.22431.12
  [Host]     : .NET 6.0.9 (6.0.922.41905), X64 RyuJIT AVX2
  Job-XHKPPE : .NET 6.0.9 (6.0.922.41905), X64 RyuJIT AVX2

Runtime=.NET 6.0  Toolchain=net6.0  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|  ObjectComparer_simple |  2.506 μs | 0.0201 μs | 0.0178 μs |
| ObjectComparer_complex | 13.745 μs | 0.1305 μs | 0.1221 μs |
