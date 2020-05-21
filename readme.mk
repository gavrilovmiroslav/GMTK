
# GMTK

## Index

[Pool](https://github.com/gavrilovmiroslav/GMTK/Pool): a bloat-free in-place generic preallocated resource pool. Requires the elements in the pool to be `referential` types; exposes simple `Acquire` and `Release` guaranteed `O(1)` methods for managing elements. Implements `IEnumerable<T>` that loops _only_ over any acquired elements.

