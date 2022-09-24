``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22000.1042/21H2)
12th Gen Intel Core i9-12900F, 1 CPU, 24 logical and 16 physical cores
.NET SDK=7.0.100-rc.1.22431.12
  [Host]     : .NET 6.0.9 (6.0.922.41905), X64 RyuJIT AVX2
  Job-KXVMCV : .NET 6.0.9 (6.0.922.41905), X64 RyuJIT AVX2

Runtime=.NET 6.0  Toolchain=net6.0  

```
|                 Method |      Mean |     Error |    StdDev |
|----------------------- |----------:|----------:|----------:|
|  ObjectComparer_simple |  2.680 μs | 0.0194 μs | 0.0162 μs |
| ObjectComparer_complex | 14.532 μs | 0.2739 μs | 0.2562 μs |
