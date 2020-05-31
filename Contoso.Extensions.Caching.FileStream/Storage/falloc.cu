#include <string.h>
#include <stdio.h>
#include <falloc.h>

///////////////////////////////////////////////////////////////////////////////
// STRUCT
// Structures used by device-size code
#pragma region STRUCT

typedef struct __align__(8)
{
	unsigned short magic;		// magic number says we're valid
	unsigned short count;		// number of chunks in sequence
	unsigned short chunkid;		// chunk ID of author
	unsigned short threadid;	// thread ID of author
} fallocChunkHeader;

typedef struct __align__(8)
{
	fallocChunkHeader *chunk;	// chunk reference
	unsigned short chunkid;		// chunk ID of author
	unsigned short threadid;	// thread ID of author
} fallocChunkRef;

typedef struct __align__(8) cuFallocDeviceHeap
{
	void *reserved;
	size_t chunkSize;
	size_t chunksLength;
	size_t chunkRefsLength; // Size of circular buffer (set up by host)
	fallocChunkRef *chunkRefs; // Start of circular buffer (set up by host)
	volatile fallocChunkRef *freeChunkPtr; // Current atomically-incremented non-wrapped offset
	volatile fallocChunkRef *retnChunkPtr; // Current atomically-incremented non-wrapped offset
	char *chunks;
} cuFallocDeviceHeap;

#pragma endregion

///////////////////////////////////////////////////////////////////////////////
// HOST SIDE
// External function definitions for host-side code
#pragma region HOST SIDE

//	cudaFallocSetDefaultHeap
extern "C" cudaError_t cudaFallocSetDefaultHeap(cudaDeviceFallocHeap &heap)
{
	return cudaMemcpyToSymbol(_defaultDeviceHeap, &heap.deviceHeap, sizeof(cuFallocDeviceHeap *));
}

//  cudaDeviceFallocCreate
//
//  Takes a buffer length to allocate, creates the memory on the device and
//  returns a pointer to it for when a kernel is called. It's up to the caller
//  to free it.
static __forceinline__ void writeChunkRefHost(fallocChunkRef *ref, fallocChunkHeader *chunk) { ref->chunk = chunk; ref->chunkid = 0; ref->threadid = 0; }
extern "C" cudaDeviceFallocHeap cudaDeviceFallocHeapCreate(size_t chunkSize, size_t length, cudaError_t *error, void *reserved)
{
	cudaError_t localError; if (!error) error = &localError;
	cudaDeviceFallocHeap heap; memset(&heap, 0, sizeof(cudaDeviceFallocHeap));
	// fix up chunkSize to include fallocChunkHeader
	chunkSize = (chunkSize + sizeof(fallocChunkHeader) + 15) & ~15;
	// fix up length to be a multiple of chunkSize
	if (!length || length % chunkSize)
		length += chunkSize - (length % chunkSize);
	size_t chunksLength = length;
	size_t chunks = (size_t)(chunksLength / chunkSize);
	if (!chunks)
		return heap;
	// fix up length to include cuFallocDeviceHeap + freechunks
	unsigned int chunkRefsLength = (unsigned int)(chunks * sizeof(fallocChunkRef));
	length = (length + chunkRefsLength + sizeof(cuFallocDeviceHeap) + 15) & ~15;
	// allocate a heap on the device and zero it
	cuFallocDeviceHeap *deviceHeap;
	if ((*error = cudaMalloc((void **)&deviceHeap, length)) != cudaSuccess || (*error = cudaMemset(deviceHeap, 0, length)) != cudaSuccess)
		return heap;
	// transfer to heap
	cuFallocDeviceHeap hostDeviceHeap;
	hostDeviceHeap.reserved = reserved;
	hostDeviceHeap.chunkSize = chunkSize;
	hostDeviceHeap.chunksLength = chunksLength;
	hostDeviceHeap.chunkRefsLength = chunkRefsLength;
	hostDeviceHeap.chunkRefs = (fallocChunkRef *)((char *)deviceHeap + sizeof(cuFallocDeviceHeap));
	hostDeviceHeap.freeChunkPtr = hostDeviceHeap.retnChunkPtr = (volatile fallocChunkRef *)hostDeviceHeap.chunkRefs;
	hostDeviceHeap.chunks = (char *)hostDeviceHeap.chunkRefs + chunkRefsLength;
	if ((*error = cudaMemcpy(deviceHeap, &hostDeviceHeap, sizeof(cuFallocDeviceHeap), cudaMemcpyHostToDevice)) != cudaSuccess)
		return heap;
	// initial chunkrefs
	char *chunk = hostDeviceHeap.chunks;
	fallocChunkRef *hostChunkRefs = new fallocChunkRef[chunks];
	unsigned int i;
	fallocChunkRef *r;
	for (i = 0, r = hostChunkRefs; i < chunks; i++, r++, chunk += chunkSize)
		writeChunkRefHost(r, (fallocChunkHeader *)chunk);
	// transfer to heap
	*error = cudaMemcpy(hostDeviceHeap.chunkRefs, hostChunkRefs, sizeof(fallocChunkRef) * chunks, cudaMemcpyHostToDevice);
	delete hostChunkRefs;
	if (*error != cudaSuccess)
		return heap;
	// return the heap
	heap.reserved = reserved;
	heap.deviceHeap = deviceHeap;
	heap.chunkSize = chunkSize;
	heap.chunksLength = chunksLength;
	heap.length = length;
	return heap;
}

//  cudaDeviceFallocHeapDestroy
//
//  Frees up the memory which we allocated
extern "C" cudaError_t cudaDeviceFallocHeapDestroy(cudaDeviceFallocHeap &heap)
{
	if (!heap.deviceHeap)
		return cudaSuccess;
	cudaError_t error = cudaFree(heap.deviceHeap); heap.deviceHeap = nullptr;
	return error;
}

#pragma endregion

///////////////////////////////////////////////////////////////////////////////
// DEVICE SIDE :: HEAP
// Heap function definitions for device-side code
#pragma region DEVICE SIDE :: HEAP

#if defined(__CUDA_ARCH__)
#define panic(fmt) { printf(fmt"\n"); asm("trap;"); }
#else
#define panic(fmt) { printf(fmt"\n"); exit(1); }
#endif  /* __CUDA_ARCH__ */

__constant__ cuFallocDeviceHeap *_defaultDeviceHeap;

#define FALLOC_MAGIC (unsigned short)0x3412 // All our headers are prefixed with a magic number so we know they're ours

static __device__ __forceinline__ void writeChunkRef(fallocChunkRef *ref, fallocChunkHeader *chunk)
{
	ref->chunk = chunk;
	ref->chunkid = gridDim.x*blockIdx.y + blockIdx.x;
	ref->threadid = blockDim.x*blockDim.y*threadIdx.z + blockDim.x*threadIdx.y + threadIdx.x;
}

static __device__ __forceinline__ void writeChunkHeader(fallocChunkHeader *hdr, unsigned short count)
{
	fallocChunkHeader header;
	header.magic = FALLOC_MAGIC;
	header.count = count;
	header.chunkid = gridDim.x*blockIdx.y + blockIdx.x;
	header.threadid = blockDim.x*blockDim.y*threadIdx.z + blockDim.x*threadIdx.y + threadIdx.x;
	*hdr = header;
}

extern "C" __device__ void *fallocGetChunk(cuFallocDeviceHeap *heap)
{
	if (!heap) heap = _defaultDeviceHeap;
	// advance circular buffer
	fallocChunkRef *chunkRefs = heap->chunkRefs;
	size_t offset = atomicAdd((unsigned int *)&heap->freeChunkPtr, sizeof(fallocChunkRef)) - (size_t)chunkRefs;
	offset %= heap->chunkRefsLength;
	fallocChunkRef *chunkRef = (fallocChunkRef *)((char *)chunkRefs + offset);
	fallocChunkHeader *chunk = chunkRef->chunk;
	writeChunkHeader(chunk, 1);
	chunkRef->chunk = nullptr;
	return (void *)((char *)chunk + sizeof(fallocChunkHeader));
}

extern "C" __device__ void fallocFreeChunk(void *obj, cuFallocDeviceHeap *heap)
{
	if (!heap) heap = _defaultDeviceHeap;
	fallocChunkHeader *chunk = (fallocChunkHeader *)((char *)obj - sizeof(fallocChunkHeader));
	if (chunk->magic != FALLOC_MAGIC || chunk->count > 1) panic("bad magic"); // bad magic or not a singular chunk
	// advance circular buffer
	fallocChunkRef *chunkRefs = heap->chunkRefs;
	size_t offset = atomicAdd((unsigned int *)&heap->retnChunkPtr, sizeof(fallocChunkRef)) - (size_t)chunkRefs;
	offset %= heap->chunkRefsLength;
	writeChunkRef((fallocChunkRef *)((char *)chunkRefs + offset), chunk);
	chunk->magic = 0;
}

#if MULTIBLOCK
/*
extern "C" __device__ inline void *fallocGetChunks(fallocHeap *heap, size_t length, size_t *allocLength = nullptr)
{
if (threadIdx.x || threadIdx.y || threadIdx.z) panic("");
size_t chunkSize = heap->chunkSize;
// fix up length to be a multiple of chunkSize
if (length % chunkSize)
length += chunkSize - (length % chunkSize);
// set length, if requested
if (allocLength)
*allocLength = length - sizeof(fallocChunkHeader);
size_t chunks = (size_t)(length / chunkSize);
if (chunks > heap->chunks) panic("");
// single, equals: fallocGetChunk
if (chunks == 1)
return fallocGetChunk(heap);
// multiple, find a contiguous chuck
size_t index = chunks;
volatile fallocChunkHeader* chunk;
volatile fallocChunkHeader* endChunk = (fallocChunkHeader*)((__int8*)heap + sizeof(fallocHeap) + (chunkSize * heap->chunks));
{ // critical
for (chunk = (fallocChunkHeader*)((__int8*)heap + sizeof(fallocHeap)); index && chunk < endChunk; chunk = (fallocChunkHeader*)((__int8*)chunk + (chunkSize * chunk->count)))
{
if (chunk->magic != FALLOC_MAGIC) panic("bad magic");
index = (chunk->next ? index - 1 : chunks);
}
if (index)
return nullptr;
// found chuck, remove from chunkRefs
endChunk = chunk;
chunk = (fallocChunkHeader*)((__int8*)chunk - (chunkSize * chunks));
for (volatile fallocChunkHeader* chunk2 = heap->chunkRefs; chunk2; chunk2 = chunk2->next)
if (chunk2 >= chunk && chunk2 <= endChunk)
chunk2->next = (chunk2->next ? chunk2->next->next : nullptr);
chunk->count = chunks;
chunk->next = nullptr;
}
return (void*)((__int8*)chunk + sizeof(fallocChunkHeader));
}

extern "C" __device__ inline void fallocFreeChunks(fallocHeap *heap, void *obj)
{
volatile fallocChunkHeader* chunk = (fallocChunkHeader*)((__int8*)obj - sizeof(fallocChunkHeader));
if (chunk->magic != FALLOC_MAGIC) panic("bad magic");
size_t chunks = chunk->count;
// single, equals: fallocFreeChunk
if (chunks == 1)
{
{ // critical
chunk->next = heap->chunkRefs;
heap->chunkRefs = chunk;
}
return;
}
// retag chunks
size_t chunkSize = heap->chunkSize;
chunk->count = 1;
while (chunks-- > 1)
{
chunk = chunk->next = (fallocChunkHeader*)((__int8*)chunk + sizeof(fallocChunkHeader) + chunkSize);
chunk->magic = FALLOC_MAGIC;
chunk->count = 1;
chunk->reserved = nullptr;
}
{ // critical
chunk->next = heap->chunkRefs;
heap->chunkRefs = chunk;
}
}
*/
#endif

#pragma endregion

///////////////////////////////////////////////////////////////////////////////
// DEVICE SIDE :: CONTEXT
// Context function definitions for device-side code
#pragma region DEVICE SIDE :: CONTEXT

const static int FALLOCNODE_SLACK = 0x10;
#define FALLOCNODE_MAGIC (unsigned short)0x7856 // All our headers are prefixed with a magic number so we know they're ours
#define FALLOCCTX_MAGIC (unsigned short)0xCC56 // All our headers are prefixed with a magic number so we know they're ours

typedef struct __align__(8) cuFallocNode
{
	struct cuFallocNode *next;
	struct cuFallocNode *nextAvailable;
	unsigned short freeOffset;
	unsigned short magic;
} fallocNode;

typedef struct __align__(8) cuFallocCtx
{
	fallocNode node;
	fallocNode *nodes;
	fallocNode *availableNodes;
	cuFallocDeviceHeap *heap;
	size_t chunkSize;
	unsigned short magic;
} cuFallocCtx;

extern "C" __device__ cuFallocCtx *fallocCreateCtx(cuFallocDeviceHeap *heap)
{
	if (!heap) heap = _defaultDeviceHeap;
	size_t chunkSize = heap->chunkSize;
	if (sizeof(cuFallocCtx) > chunkSize) panic("large chucksize");
	cuFallocCtx *ctx = (cuFallocCtx *)fallocGetChunk(heap);
	if (!ctx)
		return nullptr;
	ctx->node.magic = FALLOCNODE_MAGIC;
	ctx->node.next = nullptr;
	ctx->node.nextAvailable = nullptr;
	unsigned short freeOffset = ctx->node.freeOffset = sizeof(cuFallocCtx);
	ctx->nodes = (fallocNode *)ctx;
	ctx->availableNodes = (fallocNode *)ctx;
	ctx->heap = heap;
	ctx->chunkSize = heap->chunkSize;
	ctx->magic = FALLOCCTX_MAGIC;
	// close node
	if (freeOffset + FALLOCNODE_SLACK > chunkSize)
		ctx->availableNodes = nullptr;
	return ctx;
}

extern "C" __device__ void fallocDisposeCtx(cuFallocCtx *ctx)
{
	cuFallocDeviceHeap *heap = ctx->heap;
	for (fallocNode *node = ctx->nodes; node; node = node->next)
		fallocFreeChunk(node, heap);
}

extern "C" __device__ void *falloc(cuFallocCtx *ctx, unsigned short bytes, bool alloc)
{
	if (bytes > (ctx->chunkSize - sizeof(cuFallocCtx))) panic("size");
	// find or add available node
	fallocNode *node;
	unsigned short freeOffset;
	unsigned char hasFreeSpace;
	fallocNode *lastNode;
	for (lastNode = (fallocNode *)ctx, node = ctx->availableNodes; node; lastNode = node, node = (alloc ? node->nextAvailable : node->next))
		if (hasFreeSpace = ((freeOffset = node->freeOffset + bytes) <= ctx->chunkSize))
			break;
	if (!node || !hasFreeSpace) {
		// add node
		node = (fallocNode *)fallocGetChunk(ctx->heap);
		if (!node) panic("alloc");
		node->magic = FALLOCNODE_MAGIC;
		node->next = ctx->nodes; ctx->nodes = node;
		node->nextAvailable = (alloc ? ctx->availableNodes : nullptr); ctx->availableNodes = node;
		freeOffset = node->freeOffset = sizeof(fallocNode); 
		freeOffset += bytes;
	}
	//
	void *obj = (char *)node + node->freeOffset;
	node->freeOffset = freeOffset;
	// close node
	if (alloc && (freeOffset + FALLOCNODE_SLACK > ctx->chunkSize)) {
		if (lastNode == (fallocNode *)ctx)
			ctx->availableNodes = node->nextAvailable;
		else
			lastNode->nextAvailable = node->nextAvailable;
		node->nextAvailable = nullptr;
	}
	return obj;
}

extern "C" __device__ void *fallocRetract(cuFallocCtx *ctx, unsigned short bytes)
{
	fallocNode *node = ctx->availableNodes;
	int freeOffset = (int)node->freeOffset - bytes;
	// multi node, retract node
	if (node != &ctx->node && freeOffset < sizeof(fallocNode)) {
		node->freeOffset = sizeof(fallocNode);
		// search for previous node
		fallocNode *lastNode;
		for (lastNode = (fallocNode *)ctx, node = ctx->nodes; node; lastNode = node, node = node->next)
			if (node == ctx->availableNodes)
				break;
		node = ctx->availableNodes = lastNode;
		freeOffset = (int)node->freeOffset - bytes;
	}
	// first node && !overflow
	if (node == &ctx->node && freeOffset < sizeof(cuFallocCtx)) panic("node");
	node->freeOffset = (unsigned short)freeOffset;
	return (char *)node + freeOffset;
}

extern "C" __device__ void fallocMark(cuFallocCtx *ctx, void *&mark, unsigned short &mark2) { mark = ctx->availableNodes; mark2 = ctx->availableNodes->freeOffset; }
extern "C" __device__ bool fallocAtMark(cuFallocCtx *ctx, void *mark, unsigned short mark2) { return (mark == ctx->availableNodes && mark2 == ctx->availableNodes->freeOffset); }

#pragma endregion