/**
 * William Good
 * CS4343
 * Programming assignment 1
 * Due Oct 5, 2010
 */

#include "sorts.h"

// macros to aid in using an array to represent an almost-
// complete binary tree
#define PARENT(root, ele) (root + ((ele - root - 1) / 2))
#define LEFT(root, ele) (root + ((ele - root) * 2 + 1))
#define RIGHT(root, ele) (root + ((ele - root + 1) * 2))

static void build(int *root, int len);
static void heapify(int *root, int *target, int len);

// builds a max-heap out of the data at root
void build(int *root, int len) {
	for (int *node = root + (len / 2); node >= root; --node) {
		heapify(root, node, len);
	}
}

// moves target down the tree until it's gte its children,
// or it has no children
void heapify(int *root, int *target, int len) {
	int *end = root + len;
	int *left = LEFT(root, target);
	int *right = RIGHT(root, target);
	int *largest = target;
	if (left < end && *left > *largest) {
		largest = left;
	}
	if (right < end && *right > *largest) {
		largest = right;
	}
	if (largest != target) {
		exchange(target, largest);
		heapify(root, largest, len);
	}
}

void hsort(int *arr, int len, int *temp) {
	build(arr, len);
	int heaplen = len;
	while (heaplen > 1) {
		// while the heap part of the array is gt 1, exchange the root of the
		// heap (the largest value) with the end of the heap, decrease the
		// length of the heap by 1 and move the new root to its proper place
		// in the heap with heapify
		exchange(arr, arr + heaplen - 1);
		--heaplen;
		heapify(arr, arr, heaplen);
	}
}
