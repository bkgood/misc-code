/**
 * William Good
 * CS4343
 * Programming assignment 1
 * Due Oct 5, 2010
 */

#include <stdlib.h>
#include "sorts.h"

static int* partition(int *start, int *end); 
static void quicksort(int *start, int *end);

/**
 * Partitions data in [start, end) using a randomly chosen pivot value.
 * All data before the returned location is lte the pivot, and all data
 * after is gt the value.
 */
int* partition(int *start, int *end) {
	// pick a random pivot value, move it to the end of the array
	exchange(start + rrandom(0, end - start), end - 1);
	// this is the last element lte the pivot
	int *boundary = start - 1;
	for (int *comp = start; comp < end - 1; ++comp) {
		// loop through all the elements from the beginning to the element just
		// before the pivot
		if (*comp <= end[-1]) {
			// if the value is lte the pivot, increase the boundary by one and
			// swap the value with the lte one
			++boundary;
			exchange(boundary, comp);
		}
	}
	// move the pivot in place 
	++boundary;
	exchange(boundary, end - 1);
	return boundary;
}

/**
 * Sorts [start, end) using quicksort.
 */
void quicksort(int *start, int *end) {
	if (end - start > 1) {
		// if the list length is bigger than 1, partition the data and run
		// quicksort on the two halves
		int *pivot = partition(start, end);
		quicksort(start, pivot);
		quicksort(pivot + 1, end);
	}
}

void myqsort(int *arr, int len, int *temp) {
	quicksort(arr, arr + len);
}
