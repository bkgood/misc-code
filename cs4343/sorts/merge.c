/**
 * William Good
 * CS4343
 * Programming assignment 1
 * Due Oct 5, 2010
 */

#include <string.h> // for memcpy

#include "sorts.h"

static void merge(int *start, int *mid, int *end, int *temp);
static void mergesort(int *begin, int *end, int *temp);

// Merges two arrays together, using an aux array at temp
void merge(int *start, int *mid, int *end, int *temp) {
	const int *savedmid = mid;
	const int len = end - start;
	while (start < savedmid && mid < end) {
		// while both arrays have data, append the smallest value to the end
		// of the temp array
		if (*start <= *mid) {
			*(temp++) = *(start++);
		} else {
			*(temp++) = *(mid++);
		}
	}
	while (start < savedmid) {
		// array at mid is empty, move the rest of the data from start
		*(temp++) = *(start++);
	}
	while (mid < end) {
		// array at start is empty, move the rest of the data from mid
		*(temp++) = *(mid++);
	}
	// move the data from the temp array back to the start array
	memcpy(end - len, temp - len, sizeof(int) * len);
}

void mergesort(int *begin, int *end, int *temp) {
	if (end - begin > 1) {
		// while the array is gt 1 element in size, split the array
		// in two, run merge sort on both of those and then merge them
		int *mid = ((end - begin) / 2) + begin;
		mergesort(begin, mid, temp);
		mergesort(mid, end, temp);
		merge(begin, mid, end, temp);
	}
}

void msort(int *arr, int len, int *temp) {
	mergesort(arr, arr + len, temp);
}

