/**
 * William Good
 * CS4343
 * Programming assignment 1
 * Due Oct 5, 2010
 */

#include <stdlib.h>

// this file outlines a standard interface for the sorting algorithms
// so they can be referred to by a single function pointer type

// Sorts array of ints at arr with length len using heapsort
void hsort(int *arr, int len, int *temp);
// Sorts array of ints at arr with length len using insertion sort
void isort(int *arr, int len, int *temp);
// Sorts array of ints at arr with length len using merge sort
// merge sort requires an auxiliary storage the same size as the data
void msort(int *arr, int len, int *temp);
// Sorts array of ints at arr with length len using quick sort
void myqsort(int *arr, int len, int *temp); 

// given two non-NULL pointers to int values, switches the values pointed to
static inline void exchange(int *a, int *b) {
	int temp = *a;
	*a = *b;
	*b = temp;
}

// gives a pseudo-random number in [min, max). Seed with srandom first for
// any hope of randomness
static inline int rrandom(int min, int max) {
	return random() % (max - min) + min;
}
